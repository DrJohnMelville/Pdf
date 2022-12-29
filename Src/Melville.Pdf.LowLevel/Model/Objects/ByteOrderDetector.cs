using System;
using System.Text;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

namespace Melville.Pdf.LowLevel.Model.Objects;

public static class ByteOrderDetector {
    public static (Encoding encoding, int BomLength) DetectByteOrder(in ReadOnlySpan<byte> bytes)
    {
        if (UnicodeEncoder.BigEndian.HasUtf16BOM(bytes)) return (UnicodeEncoder.BigEndian.Encoder, 2);
        if (UnicodeEncoder.LittleEndian.HasUtf16BOM(bytes)) return (UnicodeEncoder.LittleEndian.Encoder, 2);
        if (UnicodeEncoder.Utf8.HasUtf16BOM(bytes)) return (UnicodeEncoder.Utf8.Encoder, 3);
        return (PdfDocEncoding.Instance, 0);
    }
}