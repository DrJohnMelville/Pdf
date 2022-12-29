using System;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public abstract class PdfNumber: PdfObject, IComparable<PdfNumber>
{
    public abstract long IntValue { get; }
    public abstract double DoubleValue { get; }

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
    public override long IntValue { get; }
    public override double DoubleValue => IntValue;
    public PdfInteger(long value)
    {
        IntValue = value;
    }
    public override string ToString() => IntValue.ToString();
    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    public int CompareTo(PdfInteger? other) => IntValue.CompareTo(other?.IntValue);
    /// <summary>
    /// Create a PdfInteger from a C# integer
    /// </summary>
    /// <param name="value">The desired C# value</param>
    public static implicit operator PdfInteger(int value) => new PdfInteger(value);

}
public sealed class PdfDouble : PdfNumber, IComparable<PdfDouble>
{
    public override long IntValue => (long) DoubleValue;
    public override double DoubleValue { get; }
    public PdfDouble(double value)
    {
        DoubleValue = value;
    }

    public override string ToString() => DoubleValue.ToString();
    internal override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    public int CompareTo(PdfDouble? other) => DoubleValue.CompareTo(other?.DoubleValue);
    /// <summary>
    /// Create a PdfDouble from a C# double
    /// </summary>
    /// <param name="value">The desired C# value</param>
    public static implicit operator PdfDouble(double value) => new(value);
}