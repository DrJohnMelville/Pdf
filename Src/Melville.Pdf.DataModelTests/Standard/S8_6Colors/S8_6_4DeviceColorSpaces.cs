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

    [Theory]
    [InlineData(0,0,0,1,   0,0,0)]
    [InlineData(0,0,0,0,   1,1,1)]
    [InlineData(0,1,1,0,   1,0,0)]
    [InlineData(1,0,1,0,   0,1,0)]
    [InlineData(1,1,0,0,   0,0,1)]
    [InlineData(0,0,1,0,   1,1,0)]
    [InlineData(1,0,0,0,   0,1,1)]
    [InlineData(0,1,0,0,   1,0,1)]
    public void CmykColorSpace(double c, double m, double y, double k,
        double r, double g, double b) {
        Assert.Equal(new DeviceColor(r,g,b), 
            DeviceCmyk.Instance.SetColor(new []{c,m,y,k}));
    }
}