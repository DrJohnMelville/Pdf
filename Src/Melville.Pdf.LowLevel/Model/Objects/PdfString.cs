using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Primitives;
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
        public PdfString(string str): this(str.AsExtendedAsciiBytes()) {}
        public override string ToString() => Bytes.ExtendedAsciiString();
        public bool TestEqual(string s) => TestEqual(s.AsExtendedAsciiBytes());
        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
    }
}