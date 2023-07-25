using System.Security.Cryptography;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
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

    private void SetDefault(PdfDirectValue defaultName, PdfValueArray value)
    {
        var dict = new ValueDictionaryBuilder()
            .WithItem(KnownNames.ResourcesTName, new ValueDictionaryBuilder()
                .WithItem(KnownNames.ColorSpaceTName, new ValueDictionaryBuilder()
                    .WithItem(defaultName, value)
                    .AsDictionary()).AsDictionary())
            .AsDictionary();
        page.SetupGet(i => i.LowLevel).Returns(dict);
    }

    [Fact]
    public async Task MapRgbToDeviceGrayAsync()
    {
        SetDefault(KnownNames.DefaultRGBTName, new PdfValueArray(KnownNames.DeviceGrayTName));
        var ret = await sut.ParseColorSpaceAsync(KnownNames.DeviceRGBTName);
        Assert.IsType<DeviceGray>(ret);

    }

    [Fact]
    public async Task IndexedMapsToBaseColorSpaceAsync()
    {
        SetDefault(KnownNames.DefaultRGBTName, 
            new PdfValueArray(KnownNames.IndexedTName, KnownNames.DeviceGrayTName, 1, PdfString.CreateAscii("AA")));
        var ret = await sut.ParseColorSpaceAsync(KnownNames.DeviceRGBTName);
        Assert.IsType<DeviceGray>(ret);

    }
    [Fact]
    public async Task PatternMapsToBaseColorSpaceAsync()
    {
        SetDefault(KnownNames.DefaultRGBTName, new PdfValueArray(KnownNames.PatternTName, KnownNames.DeviceGrayTName));
        var ret = await sut.ParseColorSpaceAsync(KnownNames.DeviceRGBTName);
        Assert.IsType<DeviceGray>(ret);
    }
}