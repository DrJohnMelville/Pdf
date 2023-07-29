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
        var len = EncodedLength(text);
        var ret = new byte[len ];
        FillEncodedSpan(text, ret);
        return ret;
    }

    public void FillEncodedSpan(in ReadOnlySpan<char> text, in Span<byte> ret)
    {
        preamble.AsSpan().CopyTo(ret);
        Encoder.GetBytes(text, ret[preamble.Length..]);
    }

    public int EncodedLength(ReadOnlySpan<char> text) => preamble.Length+Encoder.GetByteCount(text);

    public string GetString(byte[] bytes)
    {
        if (!HasByteOrderMark(bytes))
        {
            if (bytes.Length == 0) return "";
            throw new PdfParseException("Invalid ByteOrderMark on UtfString");
        }
        return Encoder.GetString(bytes.AsSpan(preamble.Length));
    }

    public int DecodedLength(ReadOnlySpan<byte> text) =>
        Encoder.GetCharCount(text[preamble.Length..]);

    public void FillDecodedSpan(ReadOnlySpan<byte> bytes, in Span<char> output) => Encoder.GetChars(bytes[preamble.Length..], output);

    public string? TryGetFromBOM(in ReadOnlySpan<byte> bytes) =>
        HasByteOrderMark(bytes) ? Encoder.GetString(bytes[preamble.Length..]) : null;

    public bool HasByteOrderMark(in ReadOnlySpan<byte> bytes) =>
        bytes.Length >= preamble.Length && preamble.AsSpan().SequenceEqual(bytes[..preamble.Length]);
}