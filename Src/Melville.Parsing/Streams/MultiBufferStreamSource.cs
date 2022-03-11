namespace Melville.Parsing.Streams;

public readonly struct MultiBufferStreamSource
{
    public MultiBufferStream Stream { get; }

    public MultiBufferStreamSource(MultiBufferStream stream)
    {
        Stream = stream;
    }

    public static implicit operator MultiBufferStreamSource(MultiBufferStream mbs) => new(mbs);
    public static implicit operator MultiBufferStreamSource(Stream mbs) => new(ForceMultiBufferStream(mbs));
    public static implicit operator MultiBufferStreamSource(byte[] mbs) => new(new(mbs));
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