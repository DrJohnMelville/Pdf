using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.ObjectRentals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Parsing.PipeReaders;

internal class ReusableStreamPipeReader : PipeReader, IClearable
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

    /// <summary>
    /// Reads a sequence of bytes from the from the buffer or underlying <see cref="Stream" /> using synchronous APIs.
    /// </summary>
    /// <returns>The read buffer.</returns>
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

    public override void CancelPendingRead() => 
        throw new NotSupportedException("Cancellation is not supported.");
}