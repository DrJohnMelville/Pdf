using System.IO.Pipelines;
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

    public bool TryRead(out ReadResult result)
    {
        result = CreateReadResult();
        return result.Buffer.Length > 0;
    }

    private ReadResult CreateReadResult()
    {
        return new ReadResult(data.ValidSequence(nextByte), false,
            data.DoneGrowing());
    }

    public async ValueTask<ReadResult> ReadAsync()
    {
        await data.PrepareForReadAsync(unexaminedByte).CA();
        return CreateReadResult();
    }

    public ReadResult Read()
    {
        data.PrepareForRead(unexaminedByte);
        return CreateReadResult();
    }

    public void AdvanceTo(SequencePosition consumed) => AdvanceTo(consumed, consumed);

    public void AdvanceTo(SequencePosition consumed, SequencePosition examined)
    {
        unexaminedByte = examined;
        nextByte = consumed;
        data.HasReadTo(consumed);
    }

    public void MarkSequenceAsExamined()
    {
        unexaminedByte = data.FirstInvalidPosition();
    }

    public long Position => nextByte.GlobalPosition+positionOffset;

    public void RemapCurrentPosition(long newPosition) =>
        positionOffset = newPosition - nextByte.GlobalPosition;
}