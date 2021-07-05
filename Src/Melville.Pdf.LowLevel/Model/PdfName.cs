using System;
using System.Text;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model
{
    public sealed class PdfName: IEquatable<PdfName>
    {
        private readonly byte[] name;

        public PdfName(byte[] name)
        {
            this.name = name;
        }
        
        public PdfName(string s):this(Encoding.UTF8.GetBytes(s)){}
        public override string ToString() => Encoding.UTF8.GetString(name);

        public bool Equals(PdfName? other) =>
            (!ReferenceEquals(null, other)) &&
            (ReferenceEquals(this, other) || name.AsSpan().SequenceEqual(other.name));

        public override bool Equals(object? obj) => 
            ReferenceEquals(this, obj) || obj is PdfName other && Equals(other);

        public override int GetHashCode() => FnvHash.ComputeFnvHash(name);
    }
}