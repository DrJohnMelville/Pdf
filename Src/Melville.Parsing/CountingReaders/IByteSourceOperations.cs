using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Parsing.CountingReaders;

/// <summary>
/// Simple operations on IByteSource
/// </summary>
public static class IByteSourceOperations
{
    /// <summary>
    /// Read from a byteSource until at least length bytes are available. or the end of the
    /// source is reached.
    /// </summary>
    /// <param name="source">The bytsource to read from.</param>
    /// <param name="length">The minimum required length</param>
    /// <returns>A ReadResult from the successful ReadAsync operation</returns>
    public static async ValueTask<ReadResult> ReadAtLeastAsync(this IByteSource source, int length)
    {
        while (true)
        {
            var result = await source.ReadAsync().CA();
            if (result.Buffer.Length >= length || 
                result.IsCompleted || result.IsCanceled) return result;
            source.MarkSequenceAsExamined();
        }
    }

    /// <summary>
    /// Advance the bytesource to a given index.
    /// </summary>
    /// <param name="source">The IByteSource to advance</param>
    /// <param name="position">The position to advance to.</param>
    /// <exception cref="InvalidOperationException">If the current position is greater than the desired position</exception>
    public static async ValueTask SkipForwardToAsync(this IByteSource source, long position)
    {
        if (position < source.Position)
            throw new InvalidOperationException("Cannot jump backward");
        while (source.Position < position)
        {
            var result = await source.ReadAsync().CA();
            var needed = position  - source.Position;
            source.AdvanceTo(needed>result.Buffer.Length?
                result.Buffer.End:
                result.Buffer.GetPosition(needed));
        }
    }

    /// <summary>
    /// Advance the IByteSource by a given number of bytes
    /// </summary>
    /// <param name="source">The source</param>
    /// <param name="bytes">The number of byutes to skip</param>
    public static ValueTask SkipOverAsync(this IByteSource source, int bytes) =>
        source.SkipForwardToAsync(source.Position + bytes);
}