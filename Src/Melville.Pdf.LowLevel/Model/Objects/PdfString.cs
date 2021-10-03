using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public static class HexStrings
    {
        public static string AsHex(this byte[] str) =>
            ((IEnumerable<byte>)str).AsHex();
        public static string AsHex(this IEnumerable<byte> str) =>
            string.Join(" ", str.Select(i => i.ToString("X2")));
        public static string AsHex(in this ReadOnlySpan<byte> str) =>
            string.Join(" ", str.ToArray().Select(i => i.ToString("X2")));
    }
    [DebuggerDisplay("PdfString ({ToString()}) <{Bytes.AsHex()}>")]
    public sealed class PdfString : PdfByteArrayObject
    {
        public PdfString(byte[] bytes): base(bytes) { }
        public override string ToString() => Bytes.ExtendedAsciiString();
        public bool TestEqual(string s) => TestEqual(s.AsExtendedAsciiBytes());
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

        public static PdfString CreateAscii(string str) => new(str.AsExtendedAsciiBytes());
        public String AsAsciiString() => Bytes.ExtendedAsciiString();

        public static PdfString CreateUtf16(string text) => new(Utf16BE.GetBytesWithBOM(text));
        public string AsUtf16() => Utf16BE.GetString(Bytes);
    }
}