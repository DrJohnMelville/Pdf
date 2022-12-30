using System;
using System.Text;
using System.Text.RegularExpressions;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public class PdfName: PdfByteArrayObject, IEquatable<PdfName>
{

    internal PdfName(byte[] name): base(name){}
    internal PdfName(string s):this(Encoding.UTF8.GetBytes(s)){}

    /// <inheritdoc />
    public override string ToString() => "/"+Encoding.UTF8.GetString(Bytes);
    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    /// <summary>
    /// Create a PdfName from a string.  This method will return identical objects for identical strings.
    /// </summary>
    /// <param name="s"></param>
    public static implicit operator PdfName(string s) => NameDirectory.Get(s);

    /// <inheritdoc />
    public bool Equals(PdfName? other) => Equals((PdfByteArrayObject?)other);
}