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

/// <summary>
/// This is a pipe reader that knows its location, and uses an allocation free
/// linked list of buffers
/// </summary>
public class ReusableStreamByteSource : IClearable, IByteSource
{
    private Stream? stream;
    private bool leaveOpen;
    private int desiredBufferSize;
    private bool atSourceEnd;

    private LinkedListPosition bufferStart = LinkedListPosition.Position;
    private LinkedListPosition bufferEnd = LinkedListPosition.Position;
    private LinkedListPosition examined = LinkedListPosition.Position;

    /// <summary>
    /// Rent a ReusableStreamPipeReader from the pool
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="leaveOpen">True to leave underlying stream open when this reader is clearewd.</param>
    /// <param name="desiredBufferSize">Minimum desired buffer block size in bytes.  (Default is 4096)</param>
    /// <returns>A ByteSource that is ready to read the given souorce.  Dispose of it to return it to the pool.</returns>
    public static ReusableStreamByteSource Rent(
        Stream stream, bool leaveOpen, int desiredBufferSize = 4096) =>
        ObjectPool<ReusableStreamByteSource>.Shared.Rent()
            .WithParameters(stream, leaveOpen, desiredBufferSize);


    /// <summary>
    /// Configure this reader
    /// </summary>
    /// <param name="stream">Stream to read from</param>
    /// <param name="leaveOpen">Close the stream when completed?</param>
    /// <param name="desiredBufferSize">Desired size for each block of the buffer</param>
    /// <returns>The configured reader</returns>
    public ReusableStreamByteSource WithParameters(
        Stream stream, bool leaveOpen, int desiredBufferSize = 4096)
    {
        this.stream = stream;
        this.leaveOpen = leaveOpen;
        this.desiredBufferSize = desiredBufferSize;
        var block = LinkedListNode.Rent(this.desiredBufferSize);
        bufferStart = new SequencePosition(block, 0);
        bufferEnd = bufferStart;
        examined = bufferStart;
        atSourceEnd = false;
        return this;
    }

    /// <summary>
    /// Set the index of the current position without moving the actual read position.  This is useful when
    /// a file format includes offsets from a point other than the start of the stream.
    /// </summary>
    /// <param name="startAt">The number that should be the index of the current position.</param>
    /// <returns>The configured reader.</returns>
    public ReusableStreamByteSource WithStartingPosition(long startAt)
    {
#warning -- if we might reuse the linked list this may not be ok.
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
       ObjectPool<ReusableStreamByteSource>.Shared.Return(this);
    }

    /// <summary>
    /// Clean up this reader after it is returned to the object pool
    /// </summary>
    public void Clear()
    {
        bufferStart.ClearTo(LinkedListPosition.Position); // return all nodes
        bufferStart = bufferEnd = examined = LinkedListPosition.Position;
    }

    /// <inheritdoc />
    public async ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        Debug.Assert(LinkedListNode.Empty.RunningIndex == 0);
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
        bufferStart.SequenceTo(bufferEnd);

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
    public void MarkSequenceAsExamined() => examined = bufferEnd;

    /// <inheritdoc />
    public long Position => bufferStart.GlobalPosition;
}