using System;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Model.Tags;
using Xunit;

namespace Melville.Pdf.DataModelTests.ICC;

static internal class ColorTransformTestHelpers
{
    public static void VerifyMatrixTripple(IColorTransform sut, float xOut, float yOut, float zOut)
    {
        Span<float> input = stackalloc float[] { 1, 2, 3 };
        Span<float> output = stackalloc float[3];
        sut.Transform(input, output);
        Assert.Equal(xOut, output[0]);
        Assert.Equal(yOut, output[1]);
        Assert.Equal(zOut, output[2]);
    }
}

public class StubColorTransformation:IColorTransform 
{
    public int Inputs => 3;

    public int Outputs => 3;

    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        (output[0], output[1], output[2] )= (
            input[0]+input[1]+input[2],
            10+input[0]*input[1]*input[2],
            42
        );
    }
}
