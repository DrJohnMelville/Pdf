using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using Melville.Hacks.Reflection;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.ObjectRentals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Parsing.PipeReaders;

/// <summary>
/// This is a pipe reader that knows its location, and uses an allocation free
/// linked list of buffers
/// </summary>
public class ReusableStreamPipeReader : PipeReader, IClearable, IByteSource
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
        return this;
    }

    public ReusableStreamPipeReader WithStartingPosition(long startAt)
    {
        bufferStart.RenumberCurrentPosition(startAt);
        return this;
    }


    /// <inheritdoc />
    public override void AdvanceTo(SequencePosition consumed) => this.AdvanceTo(consumed, consumed);

    /// <inheritdoc />
    public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
    {
        LinkedListPosition lpConsumed = consumed;
        bufferStart.ClearTo(lpConsumed);
        bufferStart = lpConsumed;
        this.examined = examined;
    }



    /// <inheritdoc />
    public override void Complete(Exception? exception = null)
    {
        if (!this.leaveOpen)
        {
            stream?.Dispose();
        }
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
    public override async ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
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
    public override bool TryRead(out ReadResult result)
    {
        result = CreateReadResult();
        return bufferStart != bufferEnd;
    }

    /// <inheritdoc />
    public override void CancelPendingRead() => 
        throw new NotSupportedException("Cancellation is not supported.");

    /// <inheritdoc />
    public void MarkSequenceAsExamined() => examined = bufferEnd;

    /// <inheritdoc />
    public long Position => bufferStart.GlobalPosition;
}