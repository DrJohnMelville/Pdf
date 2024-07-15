using System.Reflection.Metadata;

namespace Melville.Fonts.Type1TextParsers.EexecDecoding;

/// <summary>
/// Implements the type 1 font Eexec decoding algorithm
/// </summary>
public static class DecodeType1Encoding
{
    //Type 1 file format spec page 63 defines these encoding constants
    private static ushort c1 = 52845;
    private static ushort c2 = 22719;

    /// <summary>
    /// Decode a segment using the Eexec algorithm
    /// </summary>
    /// <param name="buffer">The bytes to decode</param>
    /// <param name="key">The key to use when decoding -- returs the next key as a ref value</param>
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
     
    /// <summary>
    /// Decode a span using the Eexec decode algorithm and chop off the etra bytes
    /// </summary>
    /// <param name="input">the text tod ecode</param>
    /// <param name="key">the key to use</param>
    /// <param name="leadBytes">The number of prepended garbage bytes</param>
    /// <returns>The decoded span</returns>         
    public static Span<byte> DecodeSpan(Span<byte> input, ushort key, int leadBytes = 4)
    {
        DecodeSegment(input, ref key);
        return input[leadBytes..];
    }
}