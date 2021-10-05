using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    [DebuggerDisplay("PdfString ({ToString()}) <{Bytes.AsHex()}>")]
    public sealed class PdfString : PdfByteArrayObject
    {
        public PdfString(byte[] bytes): base(bytes) { }
        public override string ToString() => Bytes.PdfDocEncodedString();
        public bool TestEqual(string s) => TestEqual(s.AsExtendedAsciiBytes());
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

        public static PdfString CreateAscii(string str) => new(str.AsExtendedAsciiBytes());
        public String AsAsciiString() => Bytes.PdfDocEncodedString();

        public static PdfString CreateUtf16(string text) => new(Utf16BE.GetBytesWithBOM(text));
        public string AsUtf16() => Utf16BE.GetString(Bytes);

        public static PdfString CreatePdfEncoding(string text) => new(text.AsPdfDocBytes());
        public string AsPdfDocEnccodedString() => Bytes.PdfDocEncodedString();

        public string AsTextString() => 
            Utf16BE.HasUtf16BOM(Bytes)? AsUtf16(): AsPdfDocEnccodedString();
    }
}