using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Streams;

namespace Melville.Parsing.MultiplexSources;

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
    Stream ReadFrom(long position);

#warning this can become more efficient -- then we want to get rid of ReadPipeFrom
    /// <summary>
    /// Create a multiplexed reader that begins at a given position.  The readers are threadsafe
    /// </summary>
    /// <param name="position"></param>
    IAsyncReader ReaderFrom(long position) =>
        ReadFrom(position).AsStreamReader();

    /// <summary>
    /// Return a pipe reader starting at a given point in the stream.
    /// This may be optimized to reuse buffers
    /// </summary>
    /// <param name="position">index into the source to start reading from</param>
    /// <param name="startingPosition">Sets the initial position of the ByteSource to the given position</param>
    IByteSource ReadPipeFrom(long position, long startingPosition = 0);

    /// <summary>
    /// The length of the represented data.
    /// </summary>
    long Length { get; }

    /// <summary>
    /// Returns a multiplex source with an offset from the current source
    /// </summary>
    /// <param name="offset">Number of bytes into the current source where the new
    /// source should begin</param>
    /// <returns>A, IMultiplexSource that shares data with this one, but with an offset</returns>
    IMultiplexSource OffsetFrom(uint offset);
}