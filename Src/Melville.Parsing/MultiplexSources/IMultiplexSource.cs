using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
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
    /// Create a parsing buffer starting at the given location.  The buffer disposes 
    /// of its underlying reader if needed
    /// </summary>
    /// <param name="position">Starting position to be read from.</param>
    /// <param name="bufferSize">Desired starting size of the buffer, or 0 for default.</param>
    /// <returns>A parsing buffer initialized to the given position</returns>
    ParsingBuffer ParsingBufferFrom(long position, int bufferSize = 0)
    {
        var reader = ReaderFrom(position);
        return new ParsingBuffer(reader, bufferSize).WithDisposedDependency(reader);
    }

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