using Melville.Parsing.CountingReaders;
using Melville.Parsing.LinkedLists;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.PipeReaders;
using Melville.Parsing.Streams;

namespace Melville.Parsing.MultiplexSources;

/// <summary>
/// Creates a IMultiplexedSource from a stream.  Many special cases exist.
/// </summary>
public static class MultiplexSourceFactory
{
    /// <summary>
    /// Create a multiplexed source from a stream, special casing if available.
    /// </summary>
    /// <param name="source"></param>
    /// <returns>An IMultiplexedSource that that represents the source data.</returns>
    public static IMultiplexSource Create(Stream source) =>
        source switch
        {
            IMultiplexSource ims => ims, // MultiBufferStream implements IMultiplexSource
            MemoryStream ms => Create(ms),
            FileStream fs => Create(fs),
            {CanSeek: false} => MakeStreamSeekableSource.Create(source),
            _ => new MultiplexedStream(source)
        };

    /// <summary>
    /// Create an IMultiplexedSource from a source as optimally as possible
    /// </summary>
    /// <param name="fs">The data to be accessed</param>
    /// <returns>A IMultiplexedSource representing the passed in date </returns>
    public static IMultiplexSource Create(FileStream fs) => new FileMultiplexer(fs);

    /// <summary>
    /// Create an IMultiplexedSource from a source as optimally as possible
    /// </summary>
    /// <param name="source">The data to be accessed</param>
    /// <returns>A IMultiplexedSource representing the passed in date </returns>
    public static IMultiplexSource Create(MemoryStream source) => 
        Create(MemoryStreamToMemory(source));

    /// <summary>
    /// Create an IMultiplexedSource from a source as optimally as possible
    /// </summary>
    /// <param name="source">The data to be accessed</param>
    /// <returns>A IMultiplexedSource representing the passed in date </returns>
    public static IMultiplexSource Create(Memory<byte> source) => new MultiBufferStream(source);

    private static Memory<byte> MemoryStreamToMemory(MemoryStream ms) =>
        ms.TryGetBuffer(out var buffer)
            ? buffer.Array.AsMemory(buffer.Offset, buffer.Count)
            : ms.ToArray().AsMemory();

    /// <summary>
    /// Create an IMultiplexedSource from a source as optimally as possible
    /// </summary>
    /// <param name="source">The data to be accessed</param>
    /// <returns>A IMultiplexedSource representing the passed in date </returns>
    public static IMultiplexSource Create(byte[] source) => Create(source.AsMemory());

    /// <summary>
    /// Create an IMultiplexedSource from a source as optimally as possible
    /// </summary>
    /// <param name="source">The data to be accessed</param>
    /// <returns>A IMultiplexedSource representing the passed in date </returns>
    public static IMultiplexSource Create(ReadOnlySpan<byte> source) => 
        Create(source.ToArray());

    /// <summary>
    /// Create an IMultiplexedSource from a source as optimally as possible
    /// </summary>
    /// <param name="source">The data to be accessed</param>
    /// <returns>A IMultiplexedSource representing the passed in date </returns>
    public static IMultiplexSource Create(string source)
    {
        var buffer = new byte[source.Length];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)source[i];
        }
        return Create(buffer);
    }

    /// <summary>
    /// Create an IByteSource that reads its values from a stream
    /// </summary>
    /// <param name="input">The stream to read from.</param>
    /// <param name="leaveOpen">If false, then the source stream will be closed when
    /// the reader is closed</param>
    /// <returns>an IByteSource that reads the given stream</returns>
    public static IByteSource SingleReaderForStream(Stream input, bool leaveOpen = false) =>
        SingleReadStreamBuffer.Create(input, leaveOpen).ReadPipeFrom(0);
}