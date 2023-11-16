using System;
using System.Runtime.InteropServices.Marshalling;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

/// <summary>
/// Pdf strings can have a number of encodings based on their byte order marks.
/// These methods decode pdf strings to C# strings by finding and the invoking the
/// correct decoder.
/// </summary>
public static class StringDecoder
{
    /// <summary>
    /// Get the decoded string value of a PdfDirectObject 
    /// </summary>
    /// <param name="value">The value to decode.</param>
    /// <returns>The decoded value</returns>
    public static string DecodedString(this PdfDirectObject value) =>
        value.TryGet(out StringSpanSource sss) ? DecodedString(sss) : 
            value.ToString();

    /// <summary>
    /// Decode a StringSpanSource using the decoder directed by byte order marks.
    /// </summary>
    /// <param name="value">The StringSpanSource to decode.</param>
    /// <returns>The decoded value</returns>
    public static string DecodedString(this StringSpanSource value) =>
        DecodedString(value.GetSpan());

    /// <summary>
    /// Decode a span&lt;byte&gt; using the decoder directed by byte order marks.
    /// </summary>
    /// <param name="value">The StringSpanSource to decode.</param>
    /// <returns>The decoded value</returns>
    public static string DecodedString(this Span<byte> value)
    {
        var (encoder, length) = ByteOrderDetector.DetectByteOrder(value);
        return encoder.GetString(value[length..]);
    }


    /// <summary>
    /// Get the decoded string value of a PdfDirectObject 
    /// </summary>
    /// <param name="value">The value to decode.</param>
    /// <returns>The decoded value</returns>
    public static RentedBuffer<char> DecodedBuffer(this PdfDirectObject value) =>
        (value.Get<StringSpanSource>()).DecodedBuffer();

    /// <summary>
    /// Decode a StringSpanSource using the decoder directed by byte order marks.
    /// </summary>
    /// <param name="value">The StringSpanSource to decode.</param>
    /// <returns>The decoded value</returns>
    public static RentedBuffer<char> DecodedBuffer(this StringSpanSource value) =>
        ((ReadOnlySpan<byte>)value.GetSpan()).DecodedBuffer();

    /// <summary>
    /// Decode a span&lt;byte&gt; using the decoder directed by byte order marks.
    /// </summary>
    /// <param name="value">The StringSpanSource to decode.</param>
    /// <returns>The decoded value</returns>
    public static RentedBuffer<char> DecodedBuffer(this ReadOnlySpan<byte> value)
    {
        var (encoder, tagLen) = ByteOrderDetector.DetectByteOrder(value);
        int length = encoder.GetCharCount(value[tagLen..]);
        var ret = new RentedBuffer<char>(length);
        encoder.GetChars(value[tagLen..], ret.Span);
        return ret;
    }
}