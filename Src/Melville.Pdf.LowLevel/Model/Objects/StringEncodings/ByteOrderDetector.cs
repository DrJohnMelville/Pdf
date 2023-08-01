using System;
using System.Text;

namespace Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

internal static class ByteOrderDetector {
    public static (Encoding encoding, int BomLength) DetectByteOrder(in ReadOnlySpan<byte> bytes)
    {
        if (UnicodeEncoder.BigEndian.HasByteOrderMark(bytes)) return (UnicodeEncoder.BigEndian.Encoder, 2);
        if (UnicodeEncoder.LittleEndian.HasByteOrderMark(bytes)) return (UnicodeEncoder.LittleEndian.Encoder, 2);
        if (UnicodeEncoder.Utf8.HasByteOrderMark(bytes)) return (UnicodeEncoder.Utf8.Encoder, 3);
        return (PdfDocEncoding.Instance, 0);
    }
}