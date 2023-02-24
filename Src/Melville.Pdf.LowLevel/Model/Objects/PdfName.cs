using System;
using System.Text;
using System.Text.RegularExpressions;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.ShortStrings;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

internal readonly struct PdfNameFactory : IShortStringTarget<PdfName>
{
    public PdfName Create(ShortString<NinePackedBytes> data)
    {
        throw new NotImplementedException();
    }

    public PdfName Create(ShortString<EighteenPackedBytes> data)
    {
        throw new NotImplementedException();
    }

    public PdfName Create(ShortString<ArbitraryBytes> data)
    {
        throw new NotImplementedException();
    }

    public static PdfName Create(in ReadOnlySpan<byte> data) =>
        new PdfName(data.ToArray());
}

/// <summary>
/// Represents a /PdfName construct in PDF
/// </summary>
public class PdfName: PdfByteArrayObject
{

    internal PdfName(byte[] name): base(name){}

    /// <inheritdoc />
    public override string ToString() => "/"+Encoding.UTF8.GetString(Bytes);
    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    /// <summary>
    /// Create a PdfName from a string.  This method will return identical objects for identical strings.
    /// </summary>
    /// <param name="s"></param>
    public static implicit operator PdfName(string s) => NameDirectory.Get(s);
}