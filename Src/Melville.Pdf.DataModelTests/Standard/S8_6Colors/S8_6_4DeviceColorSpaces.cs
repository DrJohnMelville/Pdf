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

}