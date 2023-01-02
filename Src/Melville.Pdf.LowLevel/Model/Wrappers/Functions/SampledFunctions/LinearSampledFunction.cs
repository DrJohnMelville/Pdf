using System;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.SampledFunctions;

internal sealed class LinearSampledFunction : SampledFunctionBase
{
    public LinearSampledFunction(ClosedInterval[] domain, ClosedInterval[] range, int[] sizes, ClosedInterval[] encode, double[] values) : base(domain, range, sizes, encode, values)
    {
    }
    protected override void InterpolateValueAtPoint(
        ReadOnlySpan<double> input, Span<int> sampleIndex, int inputIndex, double inputAsArrayIndex, 
        Span<double> result)
    {
        var lowIndex = (int)Math.Truncate(inputAsArrayIndex);
        var lowValue = EvaluateAtIndex(input, sampleIndex, inputIndex, lowIndex,
             stackalloc double[result.Length]);
        EvaluateAtIndex(input, sampleIndex, inputIndex, lowIndex + 1, result);

        var offset = inputAsArrayIndex - lowIndex;
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new ClosedInterval(0, 1)
                .MapTo(new ClosedInterval(lowValue[i], result[i]), offset);
        }
    }
}