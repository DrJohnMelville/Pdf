using System;
using Melville.Icc.Model.Tags;
using Xunit;

namespace Melville.Pdf.DataModelTests.ICC;

public class LutXComputeTest
{
    [Theory]
    [InlineData(0,0,1)]
    [InlineData(.25,.25,.75)]
    [InlineData(.75,.75,.25)]
    public void InputCurveTest(float in0, float out0, float out1)
    {
        var tag = new LutXTag(2, 2, new float[] { 0, 1, 1, 0 },
            Matrix3x3.Identity, Array.Empty<float>(), Array.Empty<float>());
        Span<float> output = stackalloc float[2];
        tag.Transform(new[]{in0, in0}.AsSpan(), output);
        Assert.Equal(out0, output[0]);
        Assert.Equal(out1, output[1]);
        
    }    
    
    [Theory]
    [InlineData(0,0,1)]
    [InlineData(.25,.25,.75)]
    [InlineData(.75,.75,.25)]
    public void OutputCurveTest(float in0, float out0, float out1)
    {
        var tag = new LutXTag(2, 2, Array.Empty<float>(),
            Matrix3x3.Identity, Array.Empty<float>(), new float[] { 0, 1, 1, 0 });
        Span<float> output = stackalloc float[2];
        tag.Transform(new[]{in0, in0}.AsSpan(), output);
        Assert.Equal(out0, output[0]);
        Assert.Equal(out1, output[1]);
        
    }
    
    [Theory]
    [InlineData(0,0,  0,0)]
    [InlineData(1,1,  5,6)]
    [InlineData(0.5, 0.5,   2.5, 3)]
    [InlineData(0.5, 0,   1, 2.5)]
    public void SimpleClut(float in0, float in1, float out0, float out1)
    {
        var sut = new LutXTag(2,2, Array.Empty<float>(), Matrix3x3.Identity, 
            new float[]{ 0, 0, 3, 1, 2, 5, 5, 6}, Array.Empty<float>());

        Span<float> input = stackalloc float[] { in0, in1 };
        Span<float> output = stackalloc float[2];
        sut.Transform(input, output);
        Assert.Equal(out0, output[0]);
        Assert.Equal(out1, output[1]);
    }

    [Fact]
    public void MatrixRotation()
    {
        var sut = new LutXTag(3, 3, Array.Empty<float>(),
            new Matrix3x3(0, 0, 1, 0, 2, 0, 1, 0, 0),
            Array.Empty<float>(), Array.Empty<float>());

        Span<float> input = stackalloc float[] { 1, 2, 3 };
        Span<float> output = stackalloc float[3];
        sut.Transform(input, output);
        Assert.Equal(3, output[0]);
        Assert.Equal(4, output[1]);
        Assert.Equal(1, output[2]);
        
    }
}