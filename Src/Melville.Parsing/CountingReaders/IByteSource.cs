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

    /// <summary>
    /// Ensures that there are unexamined bytes in the returned ReadResult.  This could
    /// return previously read, but unexamined, bytes, this could read additional data
    /// from the source, or it could report that no more bytes are available.
    /// </summary>
    /// <param name="cancellationToken">A cancellationtoken to govern this operation.</param>
    /// <returns>A ReadResult with the resulting data.</returns>
    ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ensures that there are unexamined bytes in the returned ReadResult.  This could
    /// return previously read, but unexamined, bytes, this could read additional data
    /// from the source, or it could report that no more bytes are available.
    /// </summary>
    /// <returns>A ReadResult with the resulting data.</returns>
    ReadResult Read() => TryRead(out var ret)?ret: throw new NotSupportedException("Must use an AsyncParser.");

    /// <summary>
    /// Mark the next unconsumed and unexamined byte in the source.
    /// </summary>
    /// <param name="consumed">A single position for both the next byte to examine and the next uncounsumed byte.</param>
    void AdvanceTo(SequencePosition consumed);
    
    /// <summary>
    /// Mark the next unconsumed and unexamined bytes separately.
    /// </summary>
    /// <param name="consumed">The first byte that has not been completely consumed.  The next ReadAsync will begin at
    /// this byte.</param>
    /// <param name="examined">The first byte that has not been examined.  The next ReadAsync will contain data beyond
    /// this byte.</param>
    void AdvanceTo(SequencePosition consumed, SequencePosition examined);
    
    /// <summary>
    /// Mark the entire read sequence as examined, forcing the next ReadAsync to get more data.
    /// </summary>
    void MarkSequenceAsExamined();
    
    /// <summary>
    /// The current position in the source stream.
    /// </summary>
    public long Position { get; }
}