using System;
using System.Text;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.ShortStrings;

namespace Melville.Pdf.LowLevel.Model.Primitives;

/// <summary>
/// This is an abstract base class for PDF object who'se value is an array of bytes, this
/// includes strings, names, and various tokens.
/// </summary>
public abstract class PdfByteArrayObject: PdfObject
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