using System;
using System.Buffers;
using System.Text;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

public static class Utf16BE
{
    public static readonly UnicodeEncoding UtfEncoding = new(true, true);

    public static byte[] GetBytesWithBOM(string text)
    {
        if (text.Length == 0) return Array.Empty<byte>();
        var len = 2+UtfEncoding.GetByteCount(text);
        var ret = new byte[len ];
        ret[0] = 0xFE;
        ret[1] = 0xFF;
        UtfEncoding.GetBytes(text, 0, text.Length, ret, 2);
        return ret;
    }
    public static string GetString(byte[] bytes)
    {
        if (!HasUtf16BOM(bytes))
        {
            if (bytes.Length == 0) return "";
            throw new PdfParseException("Invalid ByteOrderMark on UtfString");
        }
        return UtfEncoding.GetString(bytes.AsSpan(2));
    }

    public static bool HasUtf16BOM(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF;
}