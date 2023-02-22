using System;
using System.Text;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Primitives;

/// <summary>
/// This is an abstract base class for PDF object who'se value is an array of bytes, this
/// includes strings, names, and various tokens.
/// </summary>
public abstract class PdfByteArrayObject: PdfObject, IEquatable<PdfByteArrayObject>
{
    /// <summary>
    /// The bytes that represent the value of the ob
    /// </summary>
    public byte[] Bytes { get; }

    private protected PdfByteArrayObject(byte[] bytes)
    {
        Bytes = bytes;
    }

    /// <inheritdoc />
    public bool Equals(PdfByteArrayObject? other) =>
        other is not null &&
        (ReferenceEquals(this, other) || Bytes.AsSpan().SequenceEqual(other.Bytes));

    /// <inheritdoc />
    public override bool Equals(object? obj) => 
        ReferenceEquals(this, obj) || obj is PdfByteArrayObject other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => FnvHash.FnvHashAsInt(Bytes);

    private string DebugViewAableValue => $"{RoundTripableRepresentation(Bytes)} {Bytes.ExtendedAsciiString()}";
    private static string RoundTripableRepresentation(byte[]? inputBytes)
    {
        var sb = new StringBuilder();
        sb.Append('<');
        foreach (var item in inputBytes ?? Array.Empty<byte>())
        {
            sb.Append(item.ToString("X2"));
        }
        sb.Append('>');
        return sb.ToString();
    }
}