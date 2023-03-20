using System.IO.Pipelines;

namespace Melville.Parsing.CountingReaders;

/// <summary>
/// Represents a source of bytes to be parsed - a thin wrapper around PipeReader with a current position
/// </summary>
public interface IByteSource
{
    /// <summary>
    /// fill a read result with bytes to parse iff it can be done without blocking.
    /// </summary>
    /// <param name="result">A read result structure that receives the bytes to be parsed</param>
    /// <returns>True if there are bytes to parse, false if blockingis required.</returns>
    bool TryRead(out ReadResult result);
    ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default);
    void AdvanceTo(SequencePosition consumed);
    void AdvanceTo(SequencePosition consumed, SequencePosition examined);
    void MarkSequenceAsExamined();
    public long Position { get; }
}