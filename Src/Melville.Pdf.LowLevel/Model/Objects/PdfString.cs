﻿using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    public sealed class PdfString : PdfByteArrayObject
    {
        public PdfString(byte[] bytes): base(bytes) { }
        public PdfString(string str): this(str.AsExtendedAsciiBytes()) {}
        public override string ToString() => Bytes.ExtendedAsciiString();
        public bool TestEqual(string s) => TestEqual(s.AsExtendedAsciiBytes());
    }
}