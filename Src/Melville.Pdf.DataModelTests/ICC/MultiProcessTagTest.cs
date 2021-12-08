using System;
using Melville.Icc.Model.Tags;
using Xunit;

namespace Melville.Pdf.DataModelTests.ICC;

public class MultiProcessTagTest
{
    [Fact]
    public void SingleTransformTest()
    {
        var tag = new MultiProcessTag(new MultiProcessMatrix(1, 2, 2, 3, 0, 0));
        Assert.Equal(1, tag.Inputs);
        Assert.Equal(2, tag.Outputs);
        Span<float> output = stackalloc float[2];
        Span<float> input = stackalloc float[] { 1 };
        tag.Transform(input, output);
        Assert.Equal(2, output[0]);
        Assert.Equal(3, output[1]);
    }
    [Fact]
    public void DoubleTransformTest()
    {
        var tag = new MultiProcessTag(
            new MultiProcessMatrix(1, 1, 2, 1),
            new MultiProcessMatrix(1, 1, 2, 1));
        Span<float> output = stackalloc float[1];
        Span<float> input = stackalloc float[] { 1 };
        tag.Transform(input, output);
        Assert.Equal(7, output[0]);
    }
    [Fact]
    public void TrippleTransformTest()
    {
        var tag = new MultiProcessTag(
            new MultiProcessMatrix(1, 1, 2, 1),
            new MultiProcessMatrix(1, 1, 2, 1),
            new MultiProcessMatrix(1, 1, 2, 1));
        Span<float> output = stackalloc float[1];
        Span<float> input = stackalloc float[] { 1 };
        tag.Transform(input, output);
        Assert.Equal(15, output[0]);
    }
}