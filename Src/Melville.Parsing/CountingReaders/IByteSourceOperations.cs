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
            if (HaveEnoughBytes(length, result))
            {
                return result;
            }

            source.MarkSequenceAsExamined();
        }
    }

    private static bool HaveEnoughBytes(int length, ReadResult result)
    {
        return result.Buffer.Length >= length || 
               result.IsCompleted || result.IsCanceled;
    }

    /// <summary>
    /// Read from a byteSource until at least length bytes are available.
    /// or the end of the source is reached
    /// </summary>
    /// <param name="source">The bytsource to read from.</param>
    /// <param name="length">The minimum required length</param>
    /// <returns>The ReadResult from the successful read operation</returns>
    public static ReadResult ReadAtLeast(this IByteSource source, int length)
    {
        while (true)
        {
            var result = source.Read();
            if (HaveEnoughBytes(length, result)) return result;
            source.MarkSequenceAsExamined();
        }
    }

    /// <summary>
    /// Create a pipe at the new position.  If the new position is in the current buffer
    /// then just advance the pipe.  Otherwise dispose the pipe and create a new one from
    /// the multiplex source
    /// </summary>
    /// <param name="pipe">The original pipe</param>
    /// <param name="multiplex">The multiplex source the pipe came from</param>
    /// <param name="position">The desired position</param>
    /// <returns>A pipe, possibly the input but maby set positioned at the desired pos</returns>
    public static IByteSource PipeAtPosition(
        this IByteSource pipe, IMultiplexSource multiplex, long position)
    {
        if (pipe.TryRead(out var result))
        {
            var delta = position - pipe.Position;
            if ((ulong)delta < (ulong)result.Buffer.Length)
            {
                pipe.AdvanceTo(result.Buffer.GetPosition(delta));
                return pipe;
            }
        }
        pipe.Dispose();
        return multiplex.ReadPipeFrom(position);
    }

    /// <summary>
    /// Advance the bytesource to a given index.
    /// </summary>
    /// <param name="pipe">The IByteSource to advance</param>
    /// <param name="position">The position to advance to.</param>
    /// <exception cref="InvalidOperationException">If the current position is greater than the desired position</exception>
    public static async ValueTask SkipForwardToAsync(this IByteSource pipe, long position)
    {
        if (position < pipe.Position)
            throw new InvalidOperationException("Cannot jump backward");
        while (pipe.Position < position)
        {
            var result = await pipe.ReadAsync().CA();
            var needed = position  - pipe.Position;
            pipe.AdvanceTo(needed>result.Buffer.Length?
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