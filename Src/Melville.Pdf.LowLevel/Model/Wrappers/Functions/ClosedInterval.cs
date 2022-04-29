using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions;

public record struct ClosedInterval(double MinValue, double MaxValue)
{
    //It is legal for maxValue < minValue -- used in type 3 functions to invert a function.
    // inverted ranges are always empty and they clip oddly, but the mapto works.
    // irregular intervals have a negative size
    public double Size => MaxValue - MinValue;
    
    public double Clip(double val) =>
        val > MaxValue ? MaxValue :
        val < MinValue ? MinValue : val;

    public bool OutOfInterval(double value) => value < MinValue || value > MaxValue;

    public static readonly ClosedInterval NoRestriction = new(double.MinValue, double.MaxValue);

    public static implicit operator ClosedInterval((double Min, double Max) arg) =>
        new(arg.Min, arg.Max);
        
    public double MapTo(ClosedInterval other, double value) =>
        other.MinValue + (OffsetFromMin(value)*(other.Size/Size));
        
    private double OffsetFromMin(double value) => value - MinValue;
}

public static class ClosedIntervalOperations
{
    public static PdfArray AsPdfArray(this ICollection<ClosedInterval> source) =>
        source.AsPdfArray(source.Count);
    public static PdfArray AsPdfArray(this IEnumerable<ClosedInterval> source, int count= 0)
    {
        var numArray = new List<PdfObject>(count*2);
        foreach (var item in source)
        {
            numArray.Add(new PdfDouble(item.MinValue));
            numArray.Add(new PdfDouble(item.MaxValue));
        }
        return new PdfArray(numArray);
    }
}