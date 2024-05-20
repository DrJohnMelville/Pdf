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
            {CanSeek: false} => new MultiplexedStreamBuffer(source),
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
    public static IMultiplexSource Create(MemoryStream source) => Create(MemoryStreamToMemory(source));

    /// <summary>
    /// Create an IMultiplexedSource from a source as optimally as possible
    /// </summary>
    /// <param name="source">The data to be accessed</param>
    /// <returns>A IMultiplexedSource representing the passed in date </returns>
    public static IMultiplexSource Create(Memory<byte> source) => new MemorySource(source);

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
}