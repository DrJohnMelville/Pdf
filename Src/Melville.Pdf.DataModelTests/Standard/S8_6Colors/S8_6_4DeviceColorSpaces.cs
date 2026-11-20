using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.DataModelTests.Standard.S8_6Colors;

public class S8_6_4DeviceColorSpaces
{
    [Test]
    [Arguments(0)]
    [Arguments(0.25)]
    [Arguments(0.75)]
    [Arguments(1)]
    public void DeviceGrayTest(double value) =>
        Assert.Equal(DeviceColor.FromDoubles(value, value, value),
            DeviceGray.Instance.SetColor(new[] {value}));

    [Test]
    public void RgbColorSpace() =>
        Assert.Equal(DeviceColor.FromDoubles(0,1, 0.5), 
            DeviceRgb.Instance.SetColor(new[]{0, 1, 0.5}));
}