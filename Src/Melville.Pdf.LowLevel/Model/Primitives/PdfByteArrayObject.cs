using System;

namespace Melville.Pdf.LowLevel.Model.Primitives
{
    public abstract class PdfByteArrayObject: PdfObject, IEquatable<PdfByteArrayObject>
    {
        public byte[] Bytes { get; }

        protected PdfByteArrayObject(byte[] bytes)
        {
            Bytes = bytes;
        }
        
        public virtual bool Equals(PdfByteArrayObject? other) =>
            (!ReferenceEquals(null, other)) &&
            (ReferenceEquals(this, other) || Bytes.AsSpan().SequenceEqual(other.Bytes));

        public override bool Equals(object? obj) => 
            ReferenceEquals(this, obj) || obj is PdfByteArrayObject other && Equals(other);

        public override int GetHashCode() => FnvHash.FnvHashAsInt(Bytes);
        public bool TestEqual(ReadOnlySpan<byte> other) => other.SequenceEqual(Bytes);
    }
}