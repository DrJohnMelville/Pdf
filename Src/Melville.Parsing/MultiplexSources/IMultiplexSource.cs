using System.ComponentModel;
using System.IO.Pipelines;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.PipeReaders;

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
        IByteSource ReadPipeFrom(long position) =>
            ObjectPool<ReusableStreamPipeReader>.Shared.Rent()
                .WithParameters(ReadFrom(position), false); 

        /// <summary>
        /// The length of the represented data.
        /// </summary>
        long Length { get; }

#warning -- opportunity to optimize the memory multiplex source
        /// <summary>
        /// Returns a multiplex source with an offset from the current source
        /// </summary>
        /// <param name="offset">Number of bytes into the current source where the new
        /// source should begin</param>
        /// <returns>A, IMultiplexSource that shares data with this one, but with an offset</returns>
        IMultiplexSource OffsetFrom(uint offset) => new OffsetMultiplexSource(this, offset);
    }
} 