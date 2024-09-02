using System.Diagnostics;
using System.IO.Pipelines;
using Melville.Hacks.Reflection;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;

namespace Melville.Parsing.LinkedLists;

internal class LinkedListByteSource : IByteSource
{
    private LinkedList data = LinkedList.Empty;
    private CountedSourceTicket ticket = default;

    private LinkedListPosition nextByte;
    private LinkedListPosition unexaminedByte;
    private long positionOffset;

    public static LinkedListByteSource Create(LinkedList data, CountedSourceTicket ticket)
    {
        var ret = ObjectPool<LinkedListByteSource>.Shared.Rent();
        ret.data = data;
        ret.ticket = ticket;
        ret.unexaminedByte = ret.nextByte = data.StartPosition;
        return ret;
    }

    public void Dispose()
    {
        if (data == LinkedList.Empty) return;
        ticket.TryRelease();
        data = LinkedList.Empty;
        ObjectPool<LinkedListByteSource>.Shared.Return(this);
    }

    [Conditional("DEBUG")]
    private void AssertValidState()
    {
        Debug.Assert(data != LinkedList.Empty);
        Debug.Assert((int)(data.GetField("pendingReaders")) > 0);
        Debug.Assert(nextByte != LinkedListPosition.NullPosition);
        Debug.Assert(unexaminedByte != LinkedListPosition.NullPosition);
    }

    public bool TryRead(out ReadResult result)
    {
        AssertValidState();
        result = CreateReadResult();
        return result.Buffer.Length > 0;
    }

    private ReadResult CreateReadResult()
    {
        AssertValidState();
        return new ReadResult(data.ValidSequence(nextByte), false,
            data.DoneGrowing());
    }

    public async ValueTask<ReadResult> ReadAsync()
    {
        AssertValidState();
        await data.PrepareForReadAsync(unexaminedByte).CA();
        return CreateReadResult();
    }

    public ReadResult Read()
    {
        AssertValidState();
        data.PrepareForRead(unexaminedByte);
        return CreateReadResult();
    }

    public void AdvanceTo(SequencePosition consumed) => AdvanceTo(consumed, consumed);

    public void AdvanceTo(SequencePosition consumed, SequencePosition examined)
    {
        AssertValidState();
        unexaminedByte = examined;
        nextByte = consumed;
        data.HasReadTo(consumed);
    }

    public void MarkSequenceAsExamined()
    {
        AssertValidState();
        unexaminedByte = data.FirstInvalidPosition();
    }

    public long Position
    {
        get
        {
            AssertValidState();
            return nextByte.GlobalPosition + positionOffset;
        }
    }

    public void RemapCurrentPosition(long newPosition)
    {
        AssertValidState();
        positionOffset = newPosition - nextByte.GlobalPosition;
    }
}