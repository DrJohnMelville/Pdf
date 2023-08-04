using System;
using System.Collections.Generic;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.SampledFunctions;

internal abstract class SampledFunctionBase : PdfFunction
{
    private readonly ClosedInterval[] encode;
    private readonly MultiDimensionalArray<double> values;
    public SampledFunctionBase(
        ClosedInterval[] domain, ClosedInterval[] range, 
        IReadOnlyList<int> sizes, ClosedInterval[] encode, double[] values) : base(domain, range)
    {
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
            
        InterpolateValueAtPoint(input, sampleIndex, inputIndex, inputAsArrayIndex, result);
    }

    protected Span<double> EvaluateAtIndex(
        in ReadOnlySpan<double> input, in Span<int> sampleIndex, int inputIndex,
        int inputValue, in Span<double> result)
    {
        sampleIndex[inputIndex] = inputValue;
        ComputePartialSpan(input, sampleIndex, inputIndex-1, result);
        return result;
    }

    private void GetSample(in Span<int> sampleIndex, in Span<double> result) =>
        values.ReadValues(sampleIndex, result);

    protected abstract void InterpolateValueAtPoint(ReadOnlySpan<double> input, Span<int> sampleIndex,
        int inputIndex,
        double inputAsArrayIndex, Span<double> result);
}