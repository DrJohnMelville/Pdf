namespace Melville.Fonts.Type1TextParsers.EexecDecoding;

internal static class DecodeType1Encoding
{
    //Type 1 file format spec page 63 defines these encoding constants
    private static ushort c1 = 52845;
    private static ushort c2 = 22719;
    public static void DecodeSegment(Span<byte> buffer, ref ushort key)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            var cipher = buffer[i];
            var plain = (byte)(cipher ^ key >> 8);
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