using System;
using System.Text;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.Objects;

internal static class ByteOrderDetector {
    public static (Encoding encoding, int BomLength) DetectByteOrder(in ReadOnlySpan<byte> bytes)
    {
        if (UnicodeEncoder.BigEndian.HasByteOrderMark(bytes)) return (UnicodeEncoder.BigEndian.Encoder, 2);
        if (UnicodeEncoder.LittleEndian.HasByteOrderMark(bytes)) return (UnicodeEncoder.LittleEndian.Encoder, 2);
        if (UnicodeEncoder.Utf8.HasByteOrderMark(bytes)) return (UnicodeEncoder.Utf8.Encoder, 3);
        return (PdfDocEncoding.Instance, 0);
    }
}

public static class StringDecoder
{
    public static string DecodedString(this PdfDirectValue value) =>
        value.TryGet(out StringSpanSource sss) ? DecodedString(sss) : 
            throw new PdfParseException("Cannot decode not string");

    public static string DecodedString(this StringSpanSource value) =>
        DecodedString(value.GetSpan());

    public static string DecodedString(this Span<byte> getSpan)
    {
        var (encoder, length) = ByteOrderDetector.DetectByteOrder(getSpan);
        return encoder.GetString(getSpan[length..]);
    }
}