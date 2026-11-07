using Melville.Parsing.IStreamAdaptors;
using Melville.Parsing.StreamFilters.AsciiHex;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Pdf.ReferenceDocuments.Utility;

public static class BitStreamCreator
{
    private static readonly char[] hexDigits = { '0', '1', '2', '3','4', '5','6','7','8','9','A','B','C','D','E','F'};
    public static string HexFromBits(this Span<byte> bits) => ((ReadOnlySpan<byte>)bits).HexFromBits();
    public static string HexFromBits(this ReadOnlySpan<byte> bits)
    {
        Span<char> ret = stackalloc char[bits.Length * 2];
        int position = 0;
        foreach (var item in bits)
        {
            ret[position++] = hexDigits[item >> 4];
            ret[position++] = hexDigits[item & 0xF];
        }

        return new string(ret);
    }

    public static byte[] BitsFromHex(params ReadOnlySpan<string> sources)
    {
        return string.Join("", sources).BitsFromHex();
    }

    public static byte[] BitsFromHex(this string source)
    {
        using var bytes = new PooledBuffer<byte>(source.Length);
        Encoding.ASCII.TryGetBytes(source, bytes.AsSpan(), out var length);
        using var output = new PooledBuffer<byte>(length / 2);
        var start = output.AsSpan();
        var end = start;
        var reader = new SpanParsingBuffer(bytes.AsSpan(0, length));
        AsciiHexDecoder.Instance.FinalConvert(ref reader, ref end);
        return start[..(start.Length - end.Length)].ToArray();
        
    }

    public static byte[] BitsFromBinary(this string source) =>
        source
            .Select(BinaryValue)
            .Where(i => i >= 0)
            .Chunk(8)
            .Select(BinaryDigit)
            .ToArray();

    private static byte BinaryDigit(int[] arg)
    {
        int ret = 0;
        foreach (var digit in arg)
        {
            ret = (ret << 1) + digit;
        }
        ret <<= 8 - arg.Length;
        return (byte)ret;
    }


    private static int BinaryValue(char arg) => arg switch
    {
        '0' => 0,
        '1' => 1,
        _ => -1
    };
}