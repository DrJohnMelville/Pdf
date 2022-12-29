using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

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
               UnicodeEncoder.Utf8.TryGetFromBOM(span)??
               AsPdfDocEnccodedString();
    }

    public PdfTime AsPdfTime() => new PdfTimeParser(AsTextString().AsSpan()).AsPdfTime();
    public DateTime AsDateTime => AsPdfTime().DateTime;
    public static PdfString CreateDate(PdfTime time) => new PdfString(time.AsPdfBytes());

    public int CompareTo(PdfString? other) => 
        other == null ? 1 : Bytes.AsSpan().SequenceCompareTo(other.Bytes);

    public static PdfString CreateUtf8(string text) => 
        new(UnicodeEncoder.Utf8.GetBytesWithBOM(text));

    public string AsUtf8() => UnicodeEncoder.Utf8.GetString(Bytes);

    /// <summary>
    /// Create a PdfString from a C# string
    /// </summary>
    /// <param name="value">The desired C# value</param>
    public static implicit operator PdfString(string value) => PdfString.CreateAscii(value);
}