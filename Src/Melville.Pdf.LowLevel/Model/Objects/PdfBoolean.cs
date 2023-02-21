using System;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// Only two PdfBooleans exist.  They can be compared using object equality.  A convenience method to get the value
/// of a pdf boolean is strictly unnecessary and is implemented in terms of object equality.
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
    public static readonly PdfBoolean True = new("true"u8);
    /// <summary>
    /// Flyweight object representing all false PdfBooleans.
    /// </summary>
    public static readonly PdfBoolean False = new("false"u8);

    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    private PdfBoolean(in ReadOnlySpan<byte> tokenValue) : base(tokenValue)
    {
    }

    /// <summary>
    /// Create a PdfBoolean from a bool.
    /// </summary>
    /// <param name="value">The value of the desired PDF boolean</param>
    public static implicit operator PdfBoolean(bool value) => value ? True : False;

}