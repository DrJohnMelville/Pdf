using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.Objects2;

/// <summary>
/// Convert pdf strings and names to various formats.
/// </summary>
public static class PdfValueStringConverter
{
    /// <summary>
    /// Extract a string using the byte order marks.
    /// </summary>
    /// <param name="value">The value to convert to a string</param>
    /// <returns>Representation of the item for the </returns>
    public static string AsTextString(this PdfDirectValue value)
    {
        var span = value.Get<StringSpanSource>().GetSpan();
        return UnicodeEncoder.BigEndian.TryGetFromBOM(span) ??
               UnicodeEncoder.LittleEndian.TryGetFromBOM(span) ??
               UnicodeEncoder.Utf8.TryGetFromBOM(span) ??
               PdfDocEncodingConversion.PdfDocEncodedString(span);

    }
}