using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.LinkedLists;

namespace Melville.Parsing.Streams;

internal partial class SequenceByteReader : IByteSource
{
    [FromConstructor] private readonly ReadOnlySequence<byte> remainingSequence;
    [FromConstructor] private readonly long positionDelta;

    partial void OnConstructed()
    {
        Debug.Assert(remainingSequence.Start.GetObject() is LinkedListNode);
    }

    public void Dispose()
    {
        #warning move to a rental model
    }

    public bool TryRead(out ReadResult result)
    {
        result = new (remainingSequence, false, true);
        return remainingSequence.Length > 0;
    }

    public ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        TryRead(out var result);
        return new(result);
    }

    public void AdvanceTo(SequencePosition consumed)
    {
    }

    public void AdvanceTo(SequencePosition consumed, SequencePosition examined)
    {
        throw new NotImplementedException();
    }

    public void MarkSequenceAsExamined()
    {
        throw new NotImplementedException();
    }

    public long Position => throw new NotImplementedException();
}