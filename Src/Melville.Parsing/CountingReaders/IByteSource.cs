using System.IO.Pipelines;

namespace Melville.Parsing.CountingReaders;

/// <summary>
/// Represents a source of bytes to be parsed - a thin wrapper around PipeReader with a current position
/// </summary>
public interface IByteSource: IDisposable
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
    /// <returns>A ReadResult with the resulting data.</returns>
    ValueTask<ReadResult> ReadAsync();
    
    /// <summary>
    /// Ensures that there are unexamined bytes in the returned ReadResult.  This could
    /// return previously read, but unexamined, bytes, this could read additional data
    /// from the source, or it could report that no more bytes are available.
    /// </summary>
    /// <returns>A ReadResult with the resulting data.</returns>
    ReadResult Read() => TryRead(out var ret)?ret: 
        throw new NotSupportedException("Must use an AsyncParser.");

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

    /// <summary>
    /// This method does not change the position in the source stream, it sets the
    /// number assigned to the current position.  This is useful when parsing formats
    /// with offsets from a point other than the start of the stream.
    /// </summary>
    /// <param name="newPosition">The new value for the current position</param>
    public void RemapCurrentPosition(long newPosition);
}

public static class ByteSourceExtensions
{
    /// <summary>
    /// This is a convenience method to create a byte source and then assign it
    /// a current position
    /// </summary>
    /// <typeparam name="T">The type or the receiver</typeparam>
    /// <param name="source">The IByteSource to modify</param>
    /// <param name="newPosition">The desired index for the current position.</param>
    /// <returns>The modified IByteSource</returns>
    public static T WithCurrentPosition<T>(
        this T source, long newPosition) where T : IByteSource
    {
        source.RemapCurrentPosition(newPosition);
        return source;
    }
}