using Melville.INPC;
using Melville.Parsing.MultiplexSources;

namespace Melville.Parsing.Streams;

/// <summary>
/// This is struct is used to allow many different types to convert implicitly to a multibufferstream
/// </summary>
public readonly partial struct MultiBufferStreamSource
{
    /// <summary>
    /// Tuhe source of the bits for the source
    /// </summary>
    [FromConstructor] private readonly IMultiplexSource source;

    /// <summary>
    /// The MultiBufferStream that contains the data.
    /// </summary>
    public Stream Stream => source.ReadFrom(0);

    /// <summary>
    /// Implicitly convert a stream to a MultiBufferStreamSource
    /// </summary>
    /// <param name="mbs">The source data</param>
    public static implicit operator MultiBufferStreamSource(Stream mbs) => 
        new(MultiplexSourceFactory.Create(mbs));
    /// <summary>
    /// Implicitly convert a byte array to a MultiBufferStreamSource
    /// </summary>
    /// <param name="mbs">The source data</param>
    public static implicit operator MultiBufferStreamSource(byte[] mbs) => 
        new(MultiplexSourceFactory.Create(mbs));
    /// <summary>
    /// Implicitly convert a string to a MultiBufferStreamSource
    /// </summary>
    /// <param name="mbs">The source data</param>
    public static implicit operator MultiBufferStreamSource(string mbs) => 
        new(MultiplexSourceFactory.Create(ToBytes(mbs)));

    private static byte[] ToBytes(string mbs)
    {
        var ret = new byte[mbs.Length];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = (byte)mbs[i];
        }

        return ret;
    }
}