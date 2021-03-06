using System;
using System.Diagnostics;
using System.Text;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

[DebuggerDisplay("PdfString ({ToString()}) <{Bytes.AsHex()}>")]
public sealed class PdfString : PdfByteArrayObject, IComparable<PdfString>
{
    public static readonly PdfString Empty = new PdfString(Array.Empty<byte>());
    public PdfString(byte[] bytes): base(bytes) { }
    public override string ToString() => Bytes.PdfDocEncodedString();
    public bool TestEqual(string s) => TestEqual(s.AsExtendedAsciiBytes());
    public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    public static PdfString CreateAscii(string str) => new(str.AsExtendedAsciiBytes());
    public String AsAsciiString() => Bytes.PdfDocEncodedString();

    public static PdfString CreateUtf16(string text) => new(
        UnicodeEncoder.BigEndian.GetBytesWithBOM(text));
    public string AsUtf16() => UnicodeEncoder.BigEndian.GetString(Bytes);

    public static PdfString CreatePdfEncoding(string text) => new(text.AsPdfDocBytes());
    public string AsPdfDocEnccodedString() => Bytes.PdfDocEncodedString();

    public string AsTextString()
    {
        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(Bytes);
        return UnicodeEncoder.BigEndian.TryGetFromBOM(span) ??
               UnicodeEncoder.LittleEndian.TryGetFromBOM(span) ??
               AsPdfDocEnccodedString();
    }

    public PdfTime AsPdfTime() => Bytes.AsPdfTime();
    public DateTime AsDateTime => AsPdfTime().DateTime;
    public static PdfString CreateDate(PdfTime time) => new PdfString(time.AsPdfBytes());

    public int CompareTo(PdfString? other) => 
        other == null ? 1 : Bytes.AsSpan().SequenceCompareTo(other.Bytes);
}

public static class ByteOrderDetector {
    public static Encoding DetectByteOrder(in ReadOnlySpan<byte> bytes)
    {
        if (UnicodeEncoder.BigEndian.HasUtf16BOM(bytes)) return UnicodeEncoder.BigEndian.Encoder;
        if (UnicodeEncoder.LittleEndian.HasUtf16BOM(bytes)) return UnicodeEncoder.LittleEndian.Encoder;
        return PdfDocEncoding.Instance;
    }
}