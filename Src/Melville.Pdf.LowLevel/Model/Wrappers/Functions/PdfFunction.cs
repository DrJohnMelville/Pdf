using System;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions
{
    // Unlike most of the PDF wrappers, PdfFunction is a class instead of a struct.  This is done for
    // two principle reasons.
    // 1. Type 0, 3, and 4 functions have significant parsing requirements that require intermediate
    //    values to be saved.  We do not want to repeat this every time a function is invoked.
    // 2. This allows polymorphism over the function type, which would likely have required a strategy
    //    object anyway.
    // PdfFunction handles the input and output mapping that is common to all functions and delegates
    // actual computation to its children.
    public abstract class PdfFunction
    {
        protected ClosedInterval[] Domain { get; }
        public ClosedInterval[] Range { get; }

        protected PdfFunction(ClosedInterval[] domain, ClosedInterval[] range)
        {
            Domain = domain;
            Range = range;
        }

        public double ComputeSingleResult(double input, int desired = 0) =>
            ComputeSingleResult(InputSpan(input, stackalloc double[Domain.Length]), desired);
        public double ComputeSingleResult(in ReadOnlySpan<double> input, int desired = 0)
        {
            Span<double> ret = stackalloc double[Range.Length];
            Compute(input, ret);
            return ret[desired];
        }
        public double[] Compute(double i) => Compute(InputSpan(i, stackalloc double[Domain.Length]));
        public void Compute(double i, Span<double> result) => 
            Compute(InputSpan(i, stackalloc double[Domain.Length]), result);

        private ReadOnlySpan<double> InputSpan(double d, in Span<double> span)
        {
            span[0] = d;
            return span;
        }

        public double[] Compute(in ReadOnlySpan<double> input)
        {
            var result = new double[Range.Length];
            Compute(input, result);
            return result;
        }
        public void Compute(in ReadOnlySpan<double> input, in Span<double> result)
        {
            CheckSpanLengths(input.Length, result.Length);
            var clippedInput = OutOfDomain(input) ? 
                Clip(input, stackalloc double[Domain.Length], Domain) : input;
            ComputeOverride(clippedInput, result);
            Clip(result, result, Range);
        }

        private void CheckSpanLengths(int inputLength, int resultLength)
        {
            if (inputLength != Domain.Length)
                throw new ArgumentException("Incorrect number of arguments");
            if (resultLength != Range.Length)
                throw new ArgumentException("Incorrect number of return slots");
        }

        private bool OutOfDomain(in ReadOnlySpan<double> input)
        {
            for (int i = 0; i < Domain.Length; i++)
            {
                if (Domain[i].OutOfInterval(input[i])) return true;
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
}