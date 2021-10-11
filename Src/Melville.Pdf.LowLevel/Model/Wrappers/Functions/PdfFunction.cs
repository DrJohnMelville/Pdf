using System;
using System.Diagnostics;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions
{
    // Unlike most of the PDF wrappers, PdfFunction is a class instead of a struct.  This is done for
    // two principle reasons.
    // 1. Type 0, 3, and 4 functions have significant parsing requirements that require intermediate
    //    values to be saved.  We do not want to repeat this every time a function is invoked.
    // 2. This allows polymorphism over the function type, which would likely have required a strategy
    //    object anyway.
    // PdfFunction handles the input and output mapping that is cimmon to all functions and delegates
    // actual computation to its children.
    public abstract class PdfFunction
    {
        private readonly ClosedInterval[] domain;
        private readonly ClosedInterval[] range;

        protected PdfFunction(ClosedInterval[] domain, ClosedInterval[] range)
        {
            this.domain = domain;
            this.range = range;
        }

        public double[] Compute(in ReadOnlySpan<double> input)
        {
            var result = new double[range.Length];
            Compute(input, result);
            return result;
        }
        public void Compute(in ReadOnlySpan<double> input, in Span<double> result)
        {
            Debug.Assert(input.Length == domain.Length);
            Debug.Assert(result.Length == range.Length);
            var clippedInput = OutOfDomain(input) ? 
                Clip(input, stackalloc double[domain.Length], domain) : input;
            ComputeOverride(clippedInput, result);
            Clip(result, result, range);
        }

        private bool OutOfDomain(in ReadOnlySpan<double> input)
        {
            for (int i = 0; i < domain.Length; i++)
            {
                if (domain[i].OutOfInterval(input[i])) return true;
            }
            return false;
        }

        private ReadOnlySpan<double> Clip(in ReadOnlySpan<double> input, in Span<double> output,
            ClosedInterval[] intervals)
        {
            for (int i = 0; i < intervals.Length; i++)
            {
                output[i] = intervals[i].Clip(input[i]);
            }
            return output;
        }

        protected abstract void ComputeOverride(in ReadOnlySpan<double> input, in Span<double> result);
        
    }

    public struct ClosedInterval
    {
        private double minValue;
        private double maxValue;

        public ClosedInterval(double minValue, double maxValue)
        {
            if(minValue > maxValue) 
                throw new PdfParseException("Empty Interval");
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public double Clip(double val) =>
            val > maxValue ? maxValue :
            val < minValue ? minValue : val;

        public bool OutOfInterval(double value) => value < minValue || value > maxValue;

        public static readonly ClosedInterval NoRestriction = new(double.MinValue, double.MaxValue);
    }
}