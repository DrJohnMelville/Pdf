using System;
using Melville.Icc.Model.Tags;
using Melville.Pdf.Model.Renderers.Colors;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_6Colors;

public class XyzToDeviceColorTest
{
    private IColorTransform sut = XyzToDeviceColor.FromD50;

    // these conversion data come from http://colorizer.org/
    [Theory]
    [InlineData(255, 0, 0, 0.4125, 0.2127, 0.01933)]
    [InlineData(125, 0, 0, 0.0846, 0.0436, 0.004)]
    [InlineData(125, 127, 0, 0.1605, .1954, .0293)]
    [InlineData(125, 127, 224, .2950, .2492, .7378)]
    [InlineData(243,225, 197, .742314, .770871, .642467)]
    public void Transform(byte red, byte green, byte blue, float x, float y, float z)
    {
        Span<float> result = stackalloc float[3];
        sut.Transform(stackalloc float[]{x,y,z}, result);
        Assert.Equal(red, (int)(result[0] * 255));
        Assert.Equal(green, (int)(result[1] * 255));
        Assert.Equal(blue, (int)(result[2] * 255));
        
    }
}