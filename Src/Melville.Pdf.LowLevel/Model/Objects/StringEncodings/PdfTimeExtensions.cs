using System;
using System.Text;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

/// <summary>
/// Extension class to parse a PDFTime from a Pdf string
/// </summary>
public static class PdfTimeExtensions
{
    /// <summary>
    /// Parse a PdfTime from a PDF string or name
    /// </summary>
    /// <param name="value">A PDF string or name formatted as a time</param>
    /// <returns>The resulting PDFTime struct</returns>
    public static PdfTime AsPdfTime(this in PdfDirectObject value)
    {
        var sourceSpan = (ReadOnlySpan<byte>)value.Get<StringSpanSource>().GetSpan();
        Span<char> decoded = stackalloc char[sourceSpan.DecodedLength()];
        sourceSpan.FillDecodedChars(decoded);

        Span<byte> reEncoded = stackalloc byte[Encoding.UTF8.GetByteCount(decoded)];
        Encoding.UTF8.GetBytes(decoded, reEncoded);

        return new PdfTimeParser(reEncoded).AsPdfTime();
    }

}