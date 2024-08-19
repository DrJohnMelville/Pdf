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
    [FromConstructor] private readonly Memory<byte> source;

    /// <summary>
    /// The MultiBufferStream that contains the data.
    /// </summary>
    public Stream Stream
    {
        get
        {
            using var src = MultiplexSourceFactory.Create(source);
            return src.ReadFrom(0);
        }
    }

    /// <summary>
    /// Implicitly convert a stream to a MultiBufferStreamSource
    /// </summary>
    /// <param name="mbs">The source data</param>
    public static implicit operator MultiBufferStreamSource(Stream mbs)
    {
        var str = mbs.Length> 0 ?new MemoryStream((int)mbs.Length):new MemoryStream();
        mbs.CopyTo(str);
        return new(str.GetBuffer().AsMemory(0,(int)str.Length));
    }

    /// <summary>
    /// Implicitly convert a byte array to a MultiBufferStreamSource
    /// </summary>
    /// <param name="mbs">The source data</param>
    public static implicit operator MultiBufferStreamSource(byte[] mbs) => 
        new(mbs);
    /// <summary>
    /// Implicitly convert a string to a MultiBufferStreamSource
    /// </summary>
    /// <param name="mbs">The source data</param>
    public static implicit operator MultiBufferStreamSource(string mbs) => 
        new(ToBytes(mbs));

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