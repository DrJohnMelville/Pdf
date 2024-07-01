using System.Buffers;
using Melville.Hacks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;

namespace Melville.Fonts.Type1TextParsers;

internal class ExecDecodeStream(Stream source, ushort key) 
    : DefaultBaseStream(true, false, true)
{
    private ushort key = key;
    public override int Read(Span<byte> buffer)
    {
        var ret = source.Read(buffer);
        DecodeType1Encoding.DecodeSegment(buffer[..ret], ref key);
        return ret;
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var ret = await source.ReadAsync(buffer, cancellationToken).CA();
        DecodeType1Encoding.DecodeSegment(buffer.Span[..ret], ref key);
        return ret;
    }

    public static async Task<Stream> WrapAsync(MemoryStream memoryStream, ushort key)
    {
        var ret = new ExecDecodeStream(memoryStream, key);
        var buf = ArrayPool<byte>.Shared.Rent(4);
        await buf.FillBufferAsync(0, 4, ret).CA();
        ArrayPool<byte>.Shared.Return(buf);
        return ret;

    }
}

internal static class DecodeType1Encoding
{
    //Type 1 file format spec page 63
    private static ushort c1 = 52845;
    private static ushort c2 = 22719;
    public static void DecodeSegment(Span<byte> buffer, ref ushort key)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            var cipher = buffer[i];
            var plain = (byte)(cipher ^ (key >> 8));
            key = (ushort)((cipher + key) * c1 + c2);
            buffer[i] = plain;
        }
    }

    public static Span<byte> DecodeSpan(Span<byte> input, ushort key, int leadBytes = 4)
    {
        DecodeSegment(input, ref key);
        return input[leadBytes..];
    }
}