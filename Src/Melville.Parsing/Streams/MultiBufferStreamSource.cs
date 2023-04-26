namespace Melville.Parsing.Streams;

/// <summary>
/// Th is struct is used to allow many different types to convert implicitly to a multibufferstream
/// </summary>
public readonly struct MultiBufferStreamSource
{
    /// <summary>
    /// The MultiBufferStream that contains the data.
    /// </summary>
    public MultiBufferStream Stream { get; }

    /// <summary>
    /// Create a MultiBufferStreamSource from a MultiBufferStream
    /// </summary>
    /// <param name="stream"></param>
    public MultiBufferStreamSource(MultiBufferStream stream)
    {
        Stream = stream;
    }

    /// <summary>
    /// Implicitly convert a MultiBufferStream to a MultiBufferStreamSource
    /// </summary>
    /// <param name="mbs">The source data</param>
    public static implicit operator MultiBufferStreamSource(MultiBufferStream mbs) => new(mbs);
    /// <summary>
    /// Implicitly convert a stream to a MultiBufferStreamSource
    /// </summary>
    /// <param name="mbs">The source data</param>
    public static implicit operator MultiBufferStreamSource(Stream mbs) => new(ForceMultiBufferStream(mbs));
    /// <summary>
    /// Implicitly convert a byte array to a MultiBufferStreamSource
    /// </summary>
    /// <param name="mbs">The source data</param>
    public static implicit operator MultiBufferStreamSource(byte[] mbs) => new(new(mbs));
    /// <summary>
    /// Implicitly convert a string to a MultiBufferStreamSource
    /// </summary>
    /// <param name="mbs">The source data</param>
    public static implicit operator MultiBufferStreamSource(string mbs) => 
        new(new(ToBytes(mbs)));

    private static byte[] ToBytes(string mbs)
    {
        var ret = new byte[mbs.Length];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = (byte)mbs[i];
        }

        return ret;
    }
    private static MultiBufferStream ForceMultiBufferStream(Stream s) => 
        s is MultiBufferStream mbs ? mbs : CopyToMultiBufferStream(s);

    private static MultiBufferStream CopyToMultiBufferStream(Stream s)
    {
        var ret = new MultiBufferStream(DesiredStreamLength(s));
        s.CopyTo(ret);
        ret.Seek(0, SeekOrigin.Begin); // the returned steam must be immediately readable.
        return ret;
    }

    private static int DesiredStreamLength(Stream s) => 
        s.Length > 0?(int)s.Length:4096;

}