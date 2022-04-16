using System;
using System.Threading.Tasks;
using Melville.Icc.Model.Tags;
using Melville.Pdf.Model.Renderers.Colors;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_6Colors;

public class S8_6_4DeviceColorSpaces
{
    [Theory]
    [InlineData(0)]
    [InlineData(0.25)]
    [InlineData(0.75)]
    [InlineData(1)]
    public void DeviceGrayTest(double value) =>
        Assert.Equal(new DeviceColor(value, value, value),
            DeviceGray.Instance.SetColor(new[] {value}));

    [Fact]
    public void RgbColorSpace() =>
        Assert.Equal(new DeviceColor(0,1, 0.5), 
            DeviceRgb.Instance.SetColor(new[]{0, 1, 0.5}));
}

public class XyzToDeviceColorTest
{
    private IColorTransform sut = XyzToDeviceColor.Instance;

    // these conversion data come from http://colorizer.org/
    [Theory]
    [InlineData(255, 0, 0, 0.4125, 0.2127, 0.01933)]
    [InlineData(125, 0, 0, 0.0846, 0.0436, 0.004)]
    [InlineData(125, 127, 0, 0.1605, .1954, .0293)]
    [InlineData(125, 127, 224, .2950, .2492, .7378)]
    public void Transform(byte red, byte green, byte blue, float x, float y, float z)
    {
        Span<float> result = stackalloc float[3];
        sut.Transform(stackalloc float[]{x,y,z}, result);
        Assert.Equal(red/255.0, result[0], 2);
        Assert.Equal(green/255.0, result[1], 2);
        Assert.Equal(blue/255.0, result[2], 2);
        
    }
}