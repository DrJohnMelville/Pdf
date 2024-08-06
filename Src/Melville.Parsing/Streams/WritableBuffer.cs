using Melville.Parsing.LinkedLists;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Writers;

namespace Melville.Parsing.Streams;

/// <summary>
/// External representation of a writable buffer.
/// </summary>
public interface IWritableMultiplexSource : IMultiplexSource
{
    /// <summary>
    /// Get a stream that writes to the buffer
    /// </summary>
    Stream WritingStream();

    /// <summary>
    /// Get a counting pipe writer that writes to the buffer
    /// </summary>
    CountingPipeWriter WritingPipe();
}


/// <summary>
/// Represents a linked list buffer which can give out readers and writers.
///
/// The readers are threadsafe with respect to one another.  The caller must synchronize write operations
/// </summary>
public static class WritableBuffer
{
    /// <summary>
    /// Create a writable buffer with the desired block length
    /// </summary>
    /// <param name="desiredBlockLength">the desired block length</param>
    /// <returns></returns>
    public static IWritableMultiplexSource Create(int desiredBlockLength = 4096) =>
        MultiBufferStreamList.WritableList(desiredBlockLength);
}