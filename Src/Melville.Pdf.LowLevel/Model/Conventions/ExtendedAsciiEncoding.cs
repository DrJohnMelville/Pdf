using System;

namespace Melville.Pdf.LowLevel.Model.Conventions;

/// <summary>
/// Extended ascii is a pseudo encoding where each byte is just the low 8 bits of each character.  This is fast and effective if the
/// represented string happens to be 8 bit ascii, which is quite common in the PDF standard/
/// </summary>
public static class ExtendedAsciiEncoding
{
    /// <summary>
    /// Conver a string to an array of bytes
    /// </summary>
    /// <param name="s">The string to convert</param>
    /// <returns>An array of bytes representing the string.</returns>
    public static byte[] AsExtendedAsciiBytes(this string s)
    {
        var ret = new byte[s.Length];
        EncodeToSpan(s, ret);
        return ret;
    }

    /// <summary>
    /// Encode a string into a span of bytes
    /// </summary>
    /// <param name="input">The string to encode/</param>
    /// <param name="ret">Span that will receive the converted bytes</param>
    public static void EncodeToSpan(ReadOnlySpan<char> input, in Span<byte> ret)
    {
        for (int i = 0; i < input.Length; i++)
        {
            ret[i] = (byte)input[i];
        }
    }

    public static Span<byte> StripZerosAsync(this in Span<byte> source)
    {
        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] == 0) source[i] = (byte)'_';
        }

        return source;
    }

    /// <summary>
    /// Decode a byte array into a string.
    /// </summary>
    /// <param name="source">The bytes to be converted</param>
    /// <returns>A string representing the bytes in the input</returns>
    public static string ExtendedAsciiString(this byte[] source) =>
        ((ReadOnlySpan<byte>) source).ExtendedAsciiString();

    /// <summary>
    /// Decode a byte array into a string.
    /// </summary>
    /// <param name="source">The bytes to be converted</param>
    /// <returns>A string representing the bytes in the input</returns>
    public static unsafe string ExtendedAsciiString(this Span<byte> source) =>
        ((ReadOnlySpan<byte>)source).ExtendedAsciiString();

    /// <summary>
    /// Decode a byte array into a string.
    /// </summary>
    /// <param name="source">The bytes to be converted</param>
    /// <returns>A string representing the bytes in the input</returns>
    public static unsafe string ExtendedAsciiString(this ReadOnlySpan<byte> source)
    {
        fixed (byte* srcPtr = source)
            return string.Create(source.Length, (nint)srcPtr, ExtendedAsciiString);
    }

    private static unsafe void ExtendedAsciiString(Span<char> span, nint SourcePointerAsNativeInt)
    {
        byte* sourcePosition = (byte*)SourcePointerAsNativeInt;
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = (char)*sourcePosition++;
        }
    }
}