using System;
using Melville.Icc.Model.Tags;
using Xunit;

namespace Melville.Pdf.DataModelTests.ICC;

public class MultiProcessClutTest
{
    [Theory]
    [InlineData(0,0,  0,0)]
    [InlineData(1,1,  5,6)]
    [InlineData(0.5, 0.5,   2.5, 3)]
    [InlineData(0.5, 0,   1, 2.5)]
    public void SimpleClut(float in0, float in1, float out0, float out1)
    {
        var sut = new MultidimensionalLookupTable(new int[]{2,2}, 2, 
            0, 0, 3, 1, 2, 5, 5, 6);

        Verify(sut, in0, in1, out0, out1);
    }

    private void Verify(MultidimensionalLookupTable sut, float in0, float in1, float out0, float out1)
    {
        Span<float> input = stackalloc float[] { in0, in1 };
        Span<float> output = stackalloc float[2];
        sut.Transform(input, output);
        Assert.Equal(out0, output[0]);
        Assert.Equal(out1, output[1]);
        
    }
}