using System;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public abstract class PdfNumber: PdfObject, IComparable<PdfNumber>
{
    public abstract long IntValue { get; }
    public abstract double DoubleValue { get; }

    // public static implicit operator long(PdfNumber num) => num.IntValue;
    // public static implicit operator int(PdfNumber num) => (int)num.IntValue;
    // public static implicit operator double(PdfNumber num) => num.DoubleValue;
    public int CompareTo(PdfNumber? other) => DoubleValue.CompareTo(other?.DoubleValue);
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
    public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    public int CompareTo(PdfInteger? other) => IntValue.CompareTo(other?.IntValue);
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
    public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    public int CompareTo(PdfDouble? other) => DoubleValue.CompareTo(other?.DoubleValue);
}