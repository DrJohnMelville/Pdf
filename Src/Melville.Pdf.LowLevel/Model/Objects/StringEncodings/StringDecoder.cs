using System;
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
    /// Get the decoded string value of a PdfDirectValue 
    /// </summary>
    /// <param name="value">The value to decode.</param>
    /// <returns>The decoded value</returns>
    public static string DecodedString(this PdfDirectValue value) =>
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
}