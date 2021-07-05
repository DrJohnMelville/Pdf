using System;
using System.Text;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model
{
    public sealed class PdfName: PdfByteArrayObject, IEquatable<PdfName>
    {

        public PdfName(byte[] name):base(name) {}
        public PdfName(string s):this(Encoding.UTF8.GetBytes(s)){}
        
        public override string ToString() => Encoding.UTF8.GetString(Bytes);

        public bool Equals(PdfName? other) =>
            ((IEquatable<PdfByteArrayObject>) this).Equals(other);
    }
}