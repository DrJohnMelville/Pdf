using System;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions;

/// <summary>
/// Represents a range of values with inclusive minimum and maximum values.  This implements a lot of operations around range testing, clipping, and
/// linear interpolation needed throughout the PDF function implementation.
///
/// MaxValue can be less than MinValue, and this is used in type 3 functions to invert the function.  Inverted ranges are always empty and clip
/// oddly, but MapTo works just fine with inverted ranges.
/// </summary>
/// <param name="MinValue">The basis (ususally minimum) value of the range.</param>
/// <param name="MaxValue">The other (usually maximum) value of the range.</param>
public record struct ClosedInterval(double MinValue, double MaxValue)
{
    /// <summary>
    /// Size of the given range.
    /// </summary>
    public double Size => MaxValue - MinValue;
    
    public double Clip(double val) =>
        val > MaxValue ? MaxValue :
        val < MinValue ? MinValue : val;

    /// <summary>
    /// Text if a value is outside of the represented range.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns>True if the value is outside of the defined range, true otherwise.</returns>
    public readonly bool OutOfInterval(double value) => value < MinValue || value > MaxValue;

    /// <summary>
    /// A flyweight object foir a range that includes the entire region represented by the double data type.
    /// </summary>
    public static readonly ClosedInterval NoRestriction = new(double.MinValue, double.MaxValue);

    /// <summary>
    /// Allows closedranges to be represented by a valueType of doubles
    /// </summary>
    /// <param name="arg">A tuple of two doubles representing the minimum and maximum value of the range.</param>
    public static implicit operator ClosedInterval((double Min, double Max) arg) =>
        new(arg.Min, arg.Max);
        
    /// <summary>
    /// Map a value from this range to another range, using linear interpolation.
    /// </summary>
    /// <param name="other">The range to map to.</param>
    /// <param name="value">The value to map</param>
    /// <returns>A value that is the same fraction of the other range and the given value is of this range.</returns>
    public double MapTo(in ClosedInterval other, double value) =>
        other.MinValue + (OffsetFromMin(value)*(other.Size/Size));
    
    private double OffsetFromMin(double value) => value - MinValue;

    /// <summary>
    /// Returns the biggest range contained in both of this and another range.
    /// </summary>
    /// <param name="other">The range to intersect with</param>
    /// <returns>A range of points contained in both of this and the argument range</returns>
    public ClosedInterval Intersect(in ClosedInterval other) =>
        new(Math.Max(MinValue, other.MinValue), Math.Min(MaxValue, other.MaxValue));
}