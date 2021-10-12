using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions
{
    public struct ClosedInterval
    {
        public double MinValue { get; }
        public double MaxValue { get; }

        public ClosedInterval(double minValue, double maxValue)
        {
            if(minValue > maxValue) 
                throw new PdfParseException("Empty Interval");
            this.MinValue = minValue;
            this.MaxValue = maxValue;
        }

        public double Clip(double val) =>
            val > MaxValue ? MaxValue :
            val < MinValue ? MinValue : val;

        public bool OutOfInterval(double value) => value < MinValue || value > MaxValue;

        public static readonly ClosedInterval NoRestriction = new(double.MinValue, double.MaxValue);

        public static implicit operator ClosedInterval((double Min, double Max) arg) =>
            new(arg.Min, arg.Max);
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
}