using System.Buffers;

namespace Melville.Pdf.ReferenceDocuments.Utility;

public static class BitStreamCreator
{
    private static readonly char[] hexDigits = { '0', '1', '2', '3','4', '5','6','7','8','9','A','B','C','D','E','F'};
    public static string HexFromBits(this Span<byte> bits)
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

    public static byte[] BitsFromHex(params string[] sources)
    {
        return string.Join("", sources).BitsFromHex();
    }

    public static byte[] BitsFromHex(this string source) =>
        source
            .Select(HexValue)
            .Where(i => i >= 0)
            .Chunk(2)
            .Select(i => (byte)((i[0] << 4) | (i.Length == 2 ?i[1] : 0)))
            .ToArray();

    private static int HexValue(char character) => character switch
    {
        >= '0' and <= '9' => character - '0',
        >= 'A' and <= 'F' => 0x0A + character - 'A',
        >= 'a' and <= 'f' => 0x0A + character - 'A',
        _=> -1
    };

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