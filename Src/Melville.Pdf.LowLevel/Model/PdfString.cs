using System;
using System.Collections.Generic;

namespace Melville.Pdf.LowLevel.Model
{
    public sealed class PdfString : PdfObject
    {
        public byte[] Bytes { get; }

        public PdfString(byte[] bytes)
        {
            Bytes = bytes;
        }
        
        public PdfString(string str): this(str.AsExtendedAsciiBytes()) {}

        public override string ToString() => Bytes.ExtendedAsciiString();

        public bool TestEqual(string s) => TestEqual(s.AsExtendedAsciiBytes());
        public bool TestEqual(ReadOnlySpan<byte> other) => other.SequenceEqual(Bytes);
    }
}