using System;
using System.Text;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

public class PdfDocEncoding : Encoding
{
    public override int GetByteCount(char[] chars, int index, int count) => count;
       
    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        PdfDocEncodingConversion.FillPdfDocBytes(
            chars.AsSpan(charIndex, charCount), bytes.AsSpan(byteIndex));
        return charCount;
    }

    public override int GetCharCount(byte[] bytes, int index, int count) => count;

    public override int GetChars(
        byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
        return PdfDocEncodingConversion.GetChars(bytes.AsSpan(byteIndex, byteCount), chars.AsSpan());
    }

    public override int GetMaxByteCount(int charCount) => charCount;
    public override int GetMaxCharCount(int byteCount) => byteCount;
}