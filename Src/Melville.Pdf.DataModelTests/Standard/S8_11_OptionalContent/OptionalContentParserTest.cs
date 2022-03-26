using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.OptionalContent;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_11_OptionalContent;

public class OptionalContentParserTest
{
    private readonly PdfDictionary ocg1 = OCG("01");
    private readonly PdfDictionary ocg2 = OCG("02");
    private readonly PdfDictionary ocg3 = OCG("02");
    private static PdfDictionary OCG(string name) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.OCG)
            .WithItem(KnownNames.Name, name)
            .AsDictionary();
    
    [Fact]
    public async Task DefaultVisibility()
    {
        
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new DictionaryBuilder()
                .WithItem(KnownNames.OCGs, new PdfArray(ocg1, ocg2))
                .AsDictionary()
            );
        
        Assert.True(await sut.IsGroupVisible(ocg1));
        Assert.True(await sut.IsGroupVisible(ocg2));
    }
    [Fact]
    public async Task AllOffVisibility()
    {
        
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new DictionaryBuilder()
                .WithItem(KnownNames.OCGs, new PdfArray(ocg1, ocg2))
                .WithItem(KnownNames.D, new DictionaryBuilder()
                    .WithItem(KnownNames.BaseState, KnownNames.OFF)
                    .AsDictionary())
                .AsDictionary()
            );
        
        Assert.False(await sut.IsGroupVisible(ocg1));
        Assert.False(await sut.IsGroupVisible(ocg2));
    }
    [Fact]
    public async Task TurnOneOn()
    {
        
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new DictionaryBuilder()
                .WithItem(KnownNames.OCGs, new PdfArray(ocg1, ocg2))
                .WithItem(KnownNames.D, new DictionaryBuilder()
                    .WithItem(KnownNames.BaseState, KnownNames.OFF)
                    .WithItem(KnownNames.ON, new PdfArray(ocg1))
                    .AsDictionary())
                .AsDictionary()
            );
        
        Assert.True(await sut.IsGroupVisible(ocg1));
        Assert.False(await sut.IsGroupVisible(ocg2));
    }
}