using System;
using System.Buffers;
using System.Text;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

public sealed class UnicodeEncoder
{
    public static UnicodeEncoder BigEndian = new UnicodeEncoder(true);
    public static UnicodeEncoder LittleEndian = new UnicodeEncoder(false);
        
    public readonly UnicodeEncoding Encoder;
    private readonly byte[] preamble;

    public UnicodeEncoder(bool bigEndian)
    {
        Encoder = new UnicodeEncoding(bigEndian, true);
        preamble = Encoder.GetPreamble();
    }

    public byte[] GetBytesWithBOM(string text)
    {
        if (text.Length == 0) return Array.Empty<byte>();
        var len = preamble.Length+Encoder.GetByteCount(text);
        var ret = new byte[len ];
        preamble.CopyTo(ret, 0);
        Encoder.GetBytes(text, 0, text.Length, ret, 2);
        return ret;
    }
    public string GetString(byte[] bytes)
    {
        if (!HasUtf16BOM(bytes))
        {
            if (bytes.Length == 0) return "";
            throw new PdfParseException("Invalid ByteOrderMark on UtfString");
        }
        return Encoder.GetString(bytes.AsSpan(2));
    }

    public string? TryGetFromBOM(in ReadOnlySpan<byte> bytes) =>
        HasUtf16BOM(bytes) ? Encoder.GetString(bytes[2..]) : null;

    public bool HasUtf16BOM(in ReadOnlySpan<byte> bytes) =>
        bytes.Length > 1 && preamble.AsSpan().SequenceEqual(bytes[..preamble.Length]);
}