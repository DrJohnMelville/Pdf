using System;
using Melville.Parsing.Streams.Bases;

namespace Melville.JBig2;

public sealed class InvertingMemoryStream : DefaultBaseStream
{
    private readonly byte[] data;
    private readonly int length;
    private int offset = 0;

    public InvertingMemoryStream(byte[] data, int length): base(true, false, false)
    {
        this.data = data;
        this.length = length;
    }

    private int RemainingBytes => length - offset;

    public override int Read(Span<byte> buffer)
    {
        var len = Math.Min(buffer.Length, RemainingBytes);
        for (int i = 0; i < len; i++)
        {
            buffer[i] = (byte) ~data[offset++];
        }
        return len;
    } 
}