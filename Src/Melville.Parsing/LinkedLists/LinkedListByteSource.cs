using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;

namespace Melville.Parsing.LinkedLists;

internal class LinkedListByteSource(LinkedList data, CountedSourceTicket ticket) : IByteSource
{
    private LinkedListPosition nextByte = data.StartPosition;
    private LinkedListPosition unexaminedByte = data.StartPosition;
    private CountedSourceTicket ticket = ticket;
    private long positionOffset;

    public void Dispose()
    {
        ticket.TryRelease();
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