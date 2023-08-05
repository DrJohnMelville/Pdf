using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;

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
}