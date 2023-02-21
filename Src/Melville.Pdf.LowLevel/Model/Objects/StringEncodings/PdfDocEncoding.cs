using System;
using System.Text;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

/// <summary>
/// PdfDocEncoding is the default character encoding used in PDF
/// </summary>
[StaticSingleton]
public partial class PdfDocEncoding : Encoding
{
    /// <inheritdoc />
    public override int GetByteCount(char[] chars, int index, int count) => count;

    /// <inheritdoc />
    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        PdfDocEncodingConversion.FillPdfDocBytes(
            chars.AsSpan(charIndex, charCount), bytes.AsSpan(byteIndex));
        return charCount;
    }

    /// <inheritdoc />
    public override int GetCharCount(byte[] bytes, int index, int count) => count;

    /// <inheritdoc />
    public override int GetChars(
        byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
        return PdfDocEncodingConversion.GetChars(bytes.AsSpan(byteIndex, byteCount), chars.AsSpan());
    }

    /// <inheritdoc />
    public override int GetMaxByteCount(int charCount) => charCount;
    /// <inheritdoc />
    public override int GetMaxCharCount(int byteCount) => byteCount;
}