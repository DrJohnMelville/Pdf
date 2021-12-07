using System;
using Melville.Icc.Model.Tags;
using Xunit;

namespace Melville.Pdf.DataModelTests.ICC;

public class MultiProcessMatrixTest
{
    [Fact]
    public void MultiProcessIdentityMatrix()
    {
        var matrix = new MultiProcessMatrix(2, 2, 1, 0, 0, 1, 0, 0);
        Span<float> input = stackalloc float[] { 5, 6 };
        matrix.Transform(input, input);
        Assert.Equal(5, input[0]);
        Assert.Equal(6, input[1]);
    }
    [Fact]
    public void NonSquareMatrix()
    {
        var matrix = new MultiProcessMatrix(2, 1, 1, 1, 7);
        Span<float> input = stackalloc float[] { 5, 6 };
        var output = new float[1];
        matrix.Transform(input, output);
        Assert.Equal(18, output[0]);
    }
    [Fact]
    public void MultiProcessSwapMatrix()
    {
        var matrix = new MultiProcessMatrix(2, 2, 0, 1, 1, 0, 0, 0);
        Span<float> input = stackalloc float[] { 5, 6 };
        matrix.Transform(input, input);
        Assert.Equal(6, input[0]);
        Assert.Equal(5, input[1]);
    }
    [Fact]
    public void MultiProcesssScaleMatrix()
    {
        var matrix = new MultiProcessMatrix(2, 2, 2, 0, 0, 3, 0, 0);
        Span<float> input = stackalloc float[] { 5, 6 };
        matrix.Transform(input, input);
        Assert.Equal(10, input[0]);
        Assert.Equal(18, input[1]);
    }
    [Fact]
    public void MultiProcessTranslateMatrix()
    {
        var matrix = new MultiProcessMatrix(2, 2, 1, 0, 0, 1, 10, 20);
        Span<float> input = stackalloc float[] { 5, 6 };
        matrix.Transform(input, input);
        Assert.Equal(15, input[0]);
        Assert.Equal(26, input[1]);
    }
}