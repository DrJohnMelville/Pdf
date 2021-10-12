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
        public double Size => MaxValue - MinValue;

        public ClosedInterval(double minValue, double maxValue)
        {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            CheckValue();
        }

        private void CheckValue()
        {
            if (MinValue > MaxValue)
                throw new PdfParseException("Empty Interval");
        }

        public double Clip(double val) =>
            val > MaxValue ? MaxValue :
            val < MinValue ? MinValue : val;

        public bool OutOfInterval(double value) => value < MinValue || value > MaxValue;

        public static readonly ClosedInterval NoRestriction = new(double.MinValue, double.MaxValue);

        public static implicit operator ClosedInterval((double Min, double Max) arg) =>
            new(arg.Min, arg.Max);
        
        public double MapTo(ClosedInterval other, double value) =>
            other.MinValue + (OfsetFromMin(value)*(other.Size/Size));

        private double OfsetFromMin(double value) => value - MinValue;
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