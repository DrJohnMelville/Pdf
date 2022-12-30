using System;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public abstract class PdfNumber: PdfObject, IComparable<PdfNumber>
{
    /// <summary>
    /// The value of the number truncated to a whole numner
    /// </summary>
    public abstract long IntValue { get; }
    /// <summary>
    /// The value of the number represented.
    /// </summary>
    public abstract double DoubleValue { get; }

    /// <inheritdoc />
    public int CompareTo(PdfNumber? other) => DoubleValue.CompareTo(other?.DoubleValue);

    /// <summary>
    /// Create a PdfDouble from a C# double
    /// </summary>
    /// <param name="value">The desired C# value</param>
    public static implicit operator PdfNumber(double value) => (PdfDouble)value;

    /// <summary>
    /// Create a PdfInteger from a C# integer
    /// </summary>
    /// <param name="value">The desired C# value</param>
    public static implicit operator PdfNumber(int value) => (PdfInteger)(value);

}

public sealed class PdfInteger : PdfNumber, IComparable<PdfInteger>
{
    /// <inheritdoc />
    public override long IntValue { get; }

    /// <inheritdoc />
    public override double DoubleValue => IntValue;

    /// <summary>
    /// Creates a PdfInteger
    /// </summary>
    /// <param name="value">The value of the desired PdfInteger as a long.</param>
    public PdfInteger(long value)
    {
        IntValue = value;
    }

    /// <inheritdoc />
    public override string ToString() => IntValue.ToString();
    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    /// <inheritdoc />
    public int CompareTo(PdfInteger? other) => IntValue.CompareTo(other?.IntValue);
    /// <summary>
    /// Create a PdfInteger from a C# integer
    /// </summary>
    /// <param name="value">The desired C# value</param>
    public static implicit operator PdfInteger(int value) => new PdfInteger(value);

}
public sealed class PdfDouble : PdfNumber, IComparable<PdfDouble>
{
    /// <inheritdoc />
    public override long IntValue => (long) DoubleValue;

    /// <inheritdoc />
    public override double DoubleValue { get; }
    
    /// <summary>
    /// Creates a PdfNumber that holds its value as a double.
    /// </summary>
    /// <param name="value">The value as a number.</param>
    public PdfDouble(double value)
    {
        DoubleValue = value;
    }

    /// <inheritdoc />
    public override string ToString() => DoubleValue.ToString();
    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    /// <inheritdoc />
    public int CompareTo(PdfDouble? other) => DoubleValue.CompareTo(other?.DoubleValue);
    /// <summary>
    /// Create a PdfDouble from a C# double
    /// </summary>
    /// <param name="value">The desired C# value</param>
    public static implicit operator PdfDouble(double value) => new(value);
}