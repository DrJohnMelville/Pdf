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
    /// <summary>
    /// Flyweight instance representing all true PdfBooleans
    /// </summary>
    public static readonly PdfBoolean True = new(new byte[]{116, 114, 117, 101}); // true
    /// <summary>
    /// Flyweight object representing all false PdfBooleans.
    /// </summary>
    public static readonly PdfBoolean False = new(new byte[]{102, 97, 108, 115, 101}); // false

    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    private PdfBoolean(byte[] tokenValue) : base(tokenValue)
    {
    }

    /// <summary>
    /// Create a PdfBoolean from a bool.
    /// </summary>
    /// <param name="value">The value of the desired PDF boolean</param>
    public static implicit operator PdfBoolean(bool value) => value ? PdfBoolean.True : PdfBoolean.False;

}