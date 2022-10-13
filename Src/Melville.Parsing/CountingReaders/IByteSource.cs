using System.IO.Pipelines;

namespace Melville.Parsing.CountingReaders;

public interface IByteSource
{
    bool TryRead(out ReadResult result);
    ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default);
    void AdvanceTo(SequencePosition consumed);
    void AdvanceTo(SequencePosition consumed, SequencePosition examined);
    void MarkSequenceAsExamined();
    public long Position { get; }
}