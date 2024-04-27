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
    /// <returns></returns>
    Stream ReadFrom(long position);

    /// <summary>
    /// The length of the represented data.
    /// </summary>
    long Length { get; }
}

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
            MemoryStream ms => new MemorySource(MemoryStreamToMemory(ms)),
            FileStream fs => new FileMultiplexer(fs),
            _ => new MultiplexedStream(source)
        };

    private static Memory<byte> MemoryStreamToMemory(MemoryStream ms) =>
        ms.TryGetBuffer(out var buffer)
            ? buffer.Array.AsMemory(buffer.Offset, buffer.Count)
            : ms.ToArray().AsMemory();
}
