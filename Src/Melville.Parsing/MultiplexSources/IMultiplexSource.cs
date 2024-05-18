using System.IO.Pipelines;

namespace Melville.Parsing.MultiplexSources
{
    /// <summary>
    /// Represents a source of PDF data.  The various streams returned from this reader are threadsafe and
    /// will serialize IO operations among themselves.
    /// </summary>
    public interface IMultiplexSource : IDisposable
    {
        /// <summary>
        /// Create a multiplexed reader that begins at a given position.  The readers are threadsafe
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        Stream ReadFrom(long position);

#warning -- there is an opportunity to do optimized pipe readers here.
        /// <summary>
        /// Return a pipe reader starting at a given point in the stream.
        /// This may be optimized to reuse buffers
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        PipeReader ReadPipeFrom(long position) =>
            PipeReader.Create(ReadFrom(position));

        /// <summary>
        /// The length of the represented data.
        /// </summary>
        long Length { get; }
    }
}