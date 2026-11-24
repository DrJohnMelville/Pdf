using Melville.Parsing.LinkedLists;
using Melville.Parsing.ReaderExtensions;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Files;
using Melville.StreamInterfaces.Memory;
using Melville.StreamInterfaces.Streams;

namespace Melville.Parsing.MultiplexSources;

/// <summary>
/// Creates a IMultiplexedSource from a stream.  Many special cases exist.
/// </summary>
public static class MultiplexSourceFactory
{
    /// <summary>
    /// Create a multiplexed source from a stream, special casing if available.
    /// </summary>
    /// <param name="source">A stream containing the source data.</param>
    /// <returns>An IMultiplexedSource that that represents the source data.</returns>
    public static IMultiplexSource Create(Stream source) =>
        source switch
        {
            IMultiplexSource ims => ims, 
            IHasDataAsReadOnlyMemory hdr => Create(hdr.AsReadOnlyMemory()), 
            MemoryStream ms => Create(ms),
            FileStream fs => Create(fs),
            {CanSeek: false} => MakeStreamSeekableSource.Create(source),
            { Length: >0} => new SingleBufferSeekableMultiplexSource(source.AsStreamReader()),
            //_ => new MultiplexedStream(source)
            _ => new MultiBufferMultiplexSource(source.AsStreamReader())

        };

    public static IMultiplexSource Create(IAsyncReader source) =>
        source switch
        {
            IMultiplexSource ims => ims,
            IHasDataAsReadOnlyMemory hdr => Create(hdr.AsReadOnlyMemory()),
            IHasFileHandle hfr => new FileMultiplexer(hfr.FileHandle),
            _ when MultiplexedReaderSource.TryCreateFrom(source, out var i)  => 
                 new ReaderToMultiplexedSource(i),
            _ => new MultiBufferMultiplexSource(source)
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
    public static IMultiplexSource Create(ReadOnlyMemory<byte> source) => new MemoryMultiplexer(source);

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
}