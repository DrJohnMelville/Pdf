using System;
using System.Runtime.InteropServices;
using Melville.Hacks;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions
{
    public sealed class LinearSampledFunction : PdfFunction
    {
        private readonly int[] sizes;
        private readonly ClosedInterval[] encode;
        private readonly MultiDimensionalArray<double> values;
        public LinearSampledFunction(
            ClosedInterval[] domain, ClosedInterval[] range, 
            int[] sizes, ClosedInterval[] encode, double[] values) : base(domain, range)
        {
            this.sizes = sizes;
            this.encode = encode;
            this.values = new MultiDimensionalArray<double>(sizes, range.Length, values);
        }
        protected override void ComputeOverride(in ReadOnlySpan<double> input, in Span<double> result)
        {
            ComputePartialSpan(input, stackalloc int[input.Length], input.Length - 1, result);
        }

        private void ComputePartialSpan(
            in ReadOnlySpan<double> input, in Span<int> sampleIndex, int inputIndex,
            in Span<double> result)
        {
            if (inputIndex < 0)
            {
                GetSample(sampleIndex, result);
                return;
            }
            var inputAsArrayIndex = Domain[inputIndex].MapTo(encode[inputIndex], input[inputIndex]);
            
            var lowIndex = (int)Math.Truncate(inputAsArrayIndex);
            var highIndex = lowIndex+1;

            sampleIndex[inputIndex] = lowIndex;
            Span<double> lowValue = stackalloc double[result.Length];
            ComputePartialSpan(input, sampleIndex, inputIndex-1, lowValue);
            
            sampleIndex[inputIndex] = highIndex;
            ComputePartialSpan(input, sampleIndex, inputIndex-1, result);

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new ClosedInterval(lowIndex, highIndex)
                    .MapTo(new ClosedInterval(lowValue[i], result[i]), inputAsArrayIndex);
            }
        }

        private void GetSample(in Span<int> sampleIndex, in Span<double> result) =>
            values.ReadValues(sampleIndex, result);
    }
}