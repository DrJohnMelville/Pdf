using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_6Colors;

public class S8_6_5DefaultColorSpaces
{
    private readonly Mock<IHasPageAttributes> page = new();
    private readonly ColorSpaceFactory sut;

    public S8_6_5DefaultColorSpaces()
    {
        sut = new ColorSpaceFactory(page.Object);
    }

    private void SetDefault(PdfDirectObject defaultName, PdfArray value)
    {
        var dict = new DictionaryBuilder()
            .WithItem(KnownNames.ResourcesTName, new DictionaryBuilder()
                .WithItem(KnownNames.ColorSpaceTName, new DictionaryBuilder()
                    .WithItem(defaultName, value)
                    .AsDictionary()).AsDictionary())
            .AsDictionary();
        page.SetupGet(i => i.LowLevel).Returns(dict);
    }

    [Fact]
    public async Task MapRgbToDeviceGrayAsync()
    {
        SetDefault(KnownNames.DefaultRGBTName, new PdfArray(KnownNames.DeviceGrayTName));
        var ret = await sut.ParseColorSpaceAsync(KnownNames.DeviceRGBTName);
        Assert.IsType<DeviceGray>(ret);

    }
    [Fact]
    public async Task MapGrayToRgbAsync()
    {
        SetDefault(KnownNames.DefaultGrayTName, new PdfArray(KnownNames.DeviceRGBTName));
        var ret = await sut.ParseColorSpaceAsync(KnownNames.DeviceGrayTName);
        Assert.IsType<DeviceRgb>(ret);

    }

    [Fact]
    public async Task IndexedMapsToBaseColorSpaceAsync()
    {
        SetDefault(KnownNames.DefaultRGBTName, 
            new PdfArray(KnownNames.IndexedTName, KnownNames.DeviceGrayTName, 1, PdfDirectObject.CreateString("AA"u8)));
        var ret = await sut.ParseColorSpaceAsync(KnownNames.DeviceRGBTName);
        Assert.IsType<DeviceGray>(ret);

    }
    [Fact]
    public async Task PatternMapsToBaseColorSpaceAsync()
    {
        SetDefault(KnownNames.DefaultRGBTName, new PdfArray(KnownNames.PatternTName, KnownNames.DeviceGrayTName));
        var ret = await sut.ParseColorSpaceAsync(KnownNames.DeviceRGBTName);
        Assert.IsType<DeviceGray>(ret);
    }
}