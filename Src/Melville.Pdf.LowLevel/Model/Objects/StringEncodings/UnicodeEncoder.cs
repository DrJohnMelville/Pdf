using System;
using System.Text;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

internal sealed class UnicodeEncoder
{
    public static UnicodeEncoder BigEndian = new UnicodeEncoder(new UnicodeEncoding(true, true));
    public static UnicodeEncoder LittleEndian = new UnicodeEncoder(new UnicodeEncoding(false, true));
    public static UnicodeEncoder Utf8 = new UnicodeEncoder(new UTF8Encoding(true));
        
    public readonly Encoding Encoder;
    private readonly byte[] preamble;

    private UnicodeEncoder(Encoding encoder)
    {
        Encoder = encoder;
        preamble = Encoder.GetPreamble();
    }

    public byte[] GetBytesWithBOM(string text)
    {
        if (text.Length == 0) return Array.Empty<byte>();
        var len = preamble.Length+Encoder.GetByteCount(text);
        var ret = new byte[len ];
        preamble.CopyTo(ret, 0);
        Encoder.GetBytes(text, 0, text.Length, ret, preamble.Length);
        return ret;
    }
    public string GetString(byte[] bytes)
    {
        if (!HasUtf16BOM(bytes))
        {
            if (bytes.Length == 0) return "";
            throw new PdfParseException("Invalid ByteOrderMark on UtfString");
        }
        return Encoder.GetString(bytes.AsSpan(preamble.Length));
    }

    public string? TryGetFromBOM(in ReadOnlySpan<byte> bytes) =>
        HasUtf16BOM(bytes) ? Encoder.GetString(bytes[preamble.Length..]) : null;

    public bool HasUtf16BOM(in ReadOnlySpan<byte> bytes) =>
        bytes.Length >= preamble.Length && preamble.AsSpan().SequenceEqual(bytes[..preamble.Length]);
}