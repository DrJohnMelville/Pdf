using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using Melville.Hacks.Reflection;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.ObjectRentals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Parsing.PipeReaders;

internal partial class EnsureByteSourceDisposed: IByteSource
{
    [FromConstructor] [DelegateTo] private IByteSource inner;
    [FromConstructor] private string fileName;
    [FromConstructor] private int lineNumber;

    private string stacktrace;


    public void Dispose()
    {
        inner.Dispose();
        inner = null; // this will cause an error if I use after free
        GC.SuppressFinalize(this);
    }

    ~EnsureByteSourceDisposed()
    {
        UdpConsole.WriteLine($"""
            Undisposed reader {fileName}({lineNumber})
            """);
    }

    public static class UdpConsole
    {
        private static UdpClient? client = null;
        private static UdpClient Client
        {
            get
            {
                client ??= new UdpClient();
                return client;
            }
        }

        public static string WriteLine(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            Client.Send(bytes, bytes.Length, "127.0.0.1", 15321);
            return str;
        }
    }
}


/// <summary>
/// This is a pipe reader that knows its location, and uses an allocation free
/// linked list of buffers
/// </summary>
public class ReusableStreamPipeReader : IClearable, IByteSource
{
    private static readonly LinkedListNode EmptyNode = new LinkedListNode();

    private static readonly LinkedListPosition EmptyPosition =
        new LinkedListPosition(EmptyNode, 0);

    private Stream? stream;
    private bool leaveOpen;
    private int desiredBufferSize;
    private bool atSourceEnd;

    private LinkedListPosition bufferStart = EmptyPosition;
    private LinkedListPosition bufferEnd = EmptyPosition;
    private LinkedListPosition examined = EmptyPosition;

    public static ReusableStreamPipeReader Create(
        Stream stream, bool leaveOpen, int desiredBufferSize = 4096) =>
        ObjectPool<ReusableStreamPipeReader>.Shared.Rent()
            .WithParameters(stream, leaveOpen, desiredBufferSize);


    /// <summary>
    /// Configure this reader
    /// </summary>
    /// <param name="stream">Stream to read from</param>
    /// <param name="leaveOpen">Close the stream when completed?</param>
    /// <param name="desiredBufferSize">Desired size for each block of the buffer</param>
    /// <returns>The configured reader</returns>
    public ReusableStreamPipeReader WithParameters(
        Stream stream, bool leaveOpen, int desiredBufferSize = 4096)
    {
        this.stream = stream;
        this.leaveOpen = leaveOpen;
        this.desiredBufferSize = desiredBufferSize;
        var block = ObjectPool<LinkedListNode>.Shared.Rent()
            .With(this.desiredBufferSize);
        bufferStart = new SequencePosition(block, 0);
        bufferEnd = bufferStart;
        examined = bufferStart;
        atSourceEnd = false;
        return this;
    }

    public ReusableStreamPipeReader WithStartingPosition(long startAt)
    {
        bufferStart.RenumberCurrentPosition(startAt);
        return this;
    }


    /// <inheritdoc />
    public void AdvanceTo(SequencePosition consumed) => this.AdvanceTo(consumed, consumed);

    /// <inheritdoc />
    public void AdvanceTo(SequencePosition consumed, SequencePosition examined)
    {
        LinkedListPosition lpConsumed = consumed;
        bufferStart.ClearTo(lpConsumed);
        bufferStart = lpConsumed;
        this.examined = examined;
    }



    /// <inheritdoc />
    public void Dispose()
    {
        if (stream == null) 
            return;
        if (!this.leaveOpen)
        {
            stream.Dispose();
        }

       stream = null;
       ObjectPool<ReusableStreamPipeReader>.Shared.Return(this);
    }

    /// <summary>
    /// Clean up this reader after it is returned to the object pool
    /// </summary>
    public void Clear()
    {
        bufferStart.ClearTo(EmptyPosition); // return all nodes
        bufferStart = bufferEnd = examined = EmptyPosition;
    }

    /// <inheritdoc />
    public async ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        Debug.Assert(EmptyNode.RunningIndex == 0);
        if (AllBytesHaveBeenExamined() && !atSourceEnd && stream is not null)
        {
            (bufferEnd, atSourceEnd) =
                await bufferEnd.GetMoreBytesAsync(stream, desiredBufferSize).CA();
        }

        return CreateReadResult();
    }

    private ReadResult CreateReadResult() => 
        new(BufferAsReadOnlySequence(), false, atSourceEnd);

    private ReadOnlySequence<byte> BufferAsReadOnlySequence() =>
        new(bufferStart.Node, bufferStart.Index, bufferEnd.Node, bufferEnd.Index);

    private bool AllBytesHaveBeenExamined() => bufferEnd == examined;

    /// <inheritdoc />
    public ReadResult Read()
    {
        if (AllBytesHaveBeenExamined() && !atSourceEnd && stream is not null)
        {
            (bufferEnd, atSourceEnd) =
                bufferEnd.GetMoreBytes(stream, desiredBufferSize);
        }

        return CreateReadResult();
    }

    /// <inheritdoc />
    public  bool TryRead(out ReadResult result)
    {
        result = CreateReadResult();
        return bufferStart != bufferEnd;
    }

    /// <inheritdoc />
    public void CancelPendingRead() => 
        throw new NotSupportedException("Cancellation is not supported.");

    /// <inheritdoc />
    public void MarkSequenceAsExamined() => examined = bufferEnd;

    /// <inheritdoc />
    public long Position => bufferStart.GlobalPosition;
}