using System;
using Melville.Icc.Model.Tags;

namespace Melville.Pdf.DataModelTests.ICC;

public class MultiProcessClutTest
{
    [Test]
    [Arguments(0,0,  0,0)]
    [Arguments(1,1,  5,6)]
    [Arguments(0.5, 0.5,   2.5, 3)]
    [Arguments(0.5, 0,   1, 2.5)]
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

    [Test]
    [Arguments(0,0,0)]
    [Arguments(.1,.1,.4)]
    [Arguments(.45,.45,1)]
    [Arguments(.9,.9,.4)]
    public void TrippleClut(float in0, float out0, float out1)
    {
        var sut = new MultidimensionalLookupTable(new[] { 5 }, 2,
            0, 0, .25f, 1, .5f, 1, .75f, 1, 1, 0);

        Span<float> input = stackalloc[] { in0 };
        Span<float> output = stackalloc float[2];
        sut.Transform(input, output);
        Assert.Equal(out0, output[0], 3, MidpointRounding.ToZero);
        Assert.Equal(out1, output[1], 3, MidpointRounding.ToZero);
        
    }
}