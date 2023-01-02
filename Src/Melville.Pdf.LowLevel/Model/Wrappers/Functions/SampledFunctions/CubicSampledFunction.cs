using System;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.SampledFunctions;

internal sealed class CubicSampledFunction: SampledFunctionBase
{
    public CubicSampledFunction(
        ClosedInterval[] domain, ClosedInterval[] range, int[] sizes, ClosedInterval[] encode, 
        double[] values) : base(domain, range, sizes, encode, values)
    {
    }

    // cubic spline algorithm taken from wikipedia
    // https://en.wikipedia.org/wiki/Cubic_Hermite_spline#Interpolating_a_data_set
    protected override void InterpolateValueAtPoint(ReadOnlySpan<double> input, Span<int> sampleIndex,
        int inputIndex,
        double inputAsArrayIndex, Span<double> result)
    {
        var lowIndex = (int)Math.Truncate(inputAsArrayIndex);
        var preValue = EvaluateAtIndex(input, sampleIndex, inputIndex, lowIndex-1,
            stackalloc double[result.Length]);
        var lowValue = EvaluateAtIndex(input, sampleIndex, inputIndex, lowIndex,
            stackalloc double[result.Length]);
        var highValue = EvaluateAtIndex(input, sampleIndex, inputIndex, lowIndex+1,
            stackalloc double[result.Length]);
        var postValue = EvaluateAtIndex(input, sampleIndex, inputIndex, lowIndex+2,
            result);

        var t = inputAsArrayIndex - lowIndex;
        var tSquared = t * t;
        var tCubed = t * t * t;

        var p0Coeff = 1 + (2 * tCubed) - (3 * tSquared);
        var p1Coeff = (3 * tSquared) - (2 * tCubed);
        // the slope coefficients are predivided by four.  
        // the first division by 2 is because c is 0.5 in catmull-rom splines.
        // the second division by 2 is because we have samples at a constant sample width of 1
        // so the denominator of the slope is always 2.  I moved the division by four from the
        // slope to the coefficient to save a coupple of multiplications when evaluating a multi
        // output function.
        var m0Coeff = (t + tCubed - (2 * tSquared)) / 4;
        var m1Coeff = (tCubed - tSquared) / 4;
            
        for (int i = 0; i < result.Length; i++)
        {
            var m0 = highValue[i] - preValue[i];
            var m1 = postValue[i] - lowValue[i];
            result[i] = (p0Coeff * lowValue[i]) + (p1Coeff * highValue[i]) +
                        (m0Coeff * m0) + (m1Coeff * m1);
        }
    }
}