using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// Only two PdfBooleans exist.  They can be compared using object equality.  A convenience method to get the value
/// of a pdfboolean is strictly unnecessary and is implemented in terms of object equality.
/// </summary>
public sealed class PdfBoolean : PdfTokenValues
{
    /// <summary>
    /// True if ant only if this == PdfBoolean.True
    /// </summary>
    public bool Value => this == True;
    public static readonly PdfBoolean True = new(new byte[]{116, 114, 117, 101}); // true
    public static readonly PdfBoolean False = new(new byte[]{102, 97, 108, 115, 101}); // false

    public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    private PdfBoolean(byte[] tokenValue) : base(tokenValue)
    {
    }
}