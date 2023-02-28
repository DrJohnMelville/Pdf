using System;
using System.Text;
using System.Text.RegularExpressions;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.ShortStrings;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

internal readonly struct PdfNameFactory : IShortStringTarget<PdfName>
{
    public PdfName Create(ShortString<NinePackedBytes> data) =>
        new PdfName<NinePackedBytes>(data);

    public PdfName Create(ShortString<EighteenPackedBytes> data) =>
        new PdfName<EighteenPackedBytes>(data);

    public PdfName Create(ShortString<ArbitraryBytes> data)
    => new PdfName<ArbitraryBytes>(data);

    public static PdfName Create(in ReadOnlySpan<byte> data) =>
        data.WrapWith(new PdfNameFactory());
}

/// <summary>
/// Represents a /PdfName construct in PDF
/// </summary>
public abstract class PdfName: PdfObject
{
    /// <inheritdoc />
    //public override string ToString() => "/"+Encoding.UTF8.GetString(Bytes);
    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    /// <summary>
    /// Create a PdfName from a string.  This method will return identical objects for identical strings.
    /// </summary>
    /// <param name="s"></param>
    public static implicit operator PdfName(string s) => NameDirectory.Get(s);
    /// <summary>
    /// Check if this PdfName matches the given span.  (Minus the leading forward slash.)
    /// </summary>
    /// <param name="key">The name to compare to this PdfName</param>
    /// <returns>True if this PdfName matches the given span, false otherwise.</returns>
    public abstract bool Matches(in ReadOnlySpan<byte> key);
    /// <summary>
    /// Length of the current name, in bytes
    /// </summary>
    public abstract int Length();
    /// <summary>
    /// Fill the given span with the current name
    /// </summary>
   public abstract void Fill(Span<byte> target);
}

internal partial class PdfName<T>: PdfName where T: IPackedBytes
{
    [FromConstructor]private readonly ShortString<T> data;

    public override string ToString() => "/" + data.ValueAsString();
    public override int GetHashCode() => data.ComputeHashCode();
    public override bool Matches(in ReadOnlySpan<byte> key) => data.SameAs(key);
    public override int Length() => data.Length();
    public override void Fill(Span<byte> target) => data.Fill(target);
}