using System;
using System.Linq;
using System.Text;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Primitives;

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

    public bool IsSameAS(string s) => 
        s.Length == Bytes.Length && 
        s.Zip(Bytes, static (t, i) => t == i).All(static i=>i);

    public override bool Equals(object? obj) => 
        ReferenceEquals(this, obj) || obj is PdfByteArrayObject other && Equals(other);

    public override int GetHashCode() => FnvHash.FnvHashAsInt(Bytes);
    public bool TestEqual(ReadOnlySpan<byte> other) => other.SequenceEqual(Bytes);

    private string DebugViewAableValue => RoundTripableRepresentation(Bytes);
    private static string RoundTripableRepresentation(byte[]? inputBytes)
    {
        var sb = new StringBuilder();
        sb.Append("<");
        foreach (var item in inputBytes ?? Array.Empty<byte>())
        {
            sb.Append(item.ToString("X2"));
        }
        sb.Append(">");
        return sb.ToString();
    }
}