using System;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

/// <summary>
/// Methods to handle string decoding
/// </summary>
public static class StringDecodingImpl
{
    /// <summary>
    /// Length of a decoded string from a span of bytes.  Encoding is determined by byte order marks.
    /// </summary>
    /// <param name="input">The input bytes</param>
    /// <returns>The length, in characters, of the decoded string.</returns>
    public static int DecodedLength(this ReadOnlySpan<byte> input)
    {
        var (encoding, length) = ByteOrderDetector.DetectByteOrder(input);
        return encoding.GetDecoder().GetCharCount(input[length..], true);
    }

    /// <summary>
    /// Decode a span of bytes into a span of characters using the encoding determined by byte order marks.
    /// </summary>
    /// <param name="input">A span of bytes to decode.</param>
    /// <param name="output">Receives the decoded characters/</param>
    public static void FillDecodedChars(this ReadOnlySpan<byte> input, Span<char> output)
    {
        var (encoding, length) = ByteOrderDetector.DetectByteOrder(input);
        encoding.GetDecoder().GetChars(input[length..], output, true);
    }
}