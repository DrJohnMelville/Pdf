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
        Assert.Equal(DeviceColor.FromDoubles(value, value, value),
            DeviceGray.Instance.SetColor(new[] {value}));

    [Fact]
    public void RgbColorSpace() =>
        Assert.Equal(DeviceColor.FromDoubles(0,1, 0.5), 
            DeviceRgb.Instance.SetColor(new[]{0, 1, 0.5}));
}