using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.OptionalContent;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_11_OptionalContent;

public class OptionalContentParserTest
{
    private readonly PdfDictionary ocg1 = OCG("O1");
    private readonly PdfDictionary ocg2 = OCG("O2");
    private readonly PdfDictionary ocg3 = OCG("O3");
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
        
        Assert.False(await sut.IsGroupVisible(ocg1));
        Assert.False(await sut.IsGroupVisible(ocg2));
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

    [Fact]
    public async Task ParseSimpleOrder()
    {
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new DictionaryBuilder()
                .WithItem(KnownNames.OCGs, new PdfArray(ocg1, ocg2))
                .WithItem(KnownNames.D, new DictionaryBuilder()
                    .WithItem(KnownNames.Order, new PdfArray(ocg2, ocg1))
                    .AsDictionary())
                .AsDictionary()
        );

        var display = await sut.ConstructUiModel(sut.Configurations[0].Order);
        Assert.Equal("O2", display[0].Name);
        Assert.Equal("O1", display[1].Name);
        
    }
    [Fact]
    public async Task RadioButtons()
    {
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new DictionaryBuilder()
                .WithItem(KnownNames.OCGs, new PdfArray(ocg1, ocg2))
                .WithItem(KnownNames.D, new DictionaryBuilder()
                    .WithItem(KnownNames.Order, new PdfArray(ocg2, ocg1))
                    .WithItem(KnownNames.RBGroups, new PdfArray(new PdfArray(ocg2, ocg1)))
                    .AsDictionary())
                .AsDictionary()
        );

        var display = await sut.ConstructUiModel(sut.Configurations[0].Order);
        var disp =await sut.ConstructUiModel(sut.SelectedConfiguration!.Order);
        disp[0].Visible = true;
        Assert.False(disp[1].Visible);
        disp[1].Visible = true;
        Assert.False(disp[0].Visible);

        // setting to false does not reset
        disp[0].Visible = false;
        Assert.True(disp[1].Visible);

    }
    [Fact]
    public async Task ParseOrderWithTitle()
    {
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new DictionaryBuilder()
                .WithItem(KnownNames.OCGs, new PdfArray(ocg1, ocg2))
                .WithItem(KnownNames.D, new DictionaryBuilder()
                    .WithItem(KnownNames.Order, new PdfArray( 
                        new PdfArray(PdfString.CreateUtf16("Title"),ocg2, ocg1)))
                    .AsDictionary())
                .AsDictionary()
        );

        var display = await sut.ConstructUiModel(sut.Configurations[0].Order);
        Assert.Equal("Title", display[0].Name);
        Assert.False(display[0].ShowCheck);
        
        Assert.Equal("O2", display[0].Children[0].Name);
        Assert.Equal("O1", display[0].Children[1].Name);
        
    }
    [Fact]
    public async Task ParseOrderWithSuperGroup()
    {
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new DictionaryBuilder()
                .WithItem(KnownNames.OCGs, new PdfArray(ocg1, ocg2,ocg3))
                .WithItem(KnownNames.D, new DictionaryBuilder()
                    .WithItem(KnownNames.Order, new PdfArray( 
                        new PdfArray(ocg3,ocg2, ocg1)))
                    .AsDictionary())
                .AsDictionary()
        );

        var display = await sut.ConstructUiModel(sut.Configurations[0].Order);
        Assert.Equal("O3", display[0].Name);
        Assert.True(display[0].ShowCheck);
        
        Assert.Equal("O2", display[0].Children[0].Name);
        Assert.Equal("O1", display[0].Children[1].Name);
        
    }

    public static object[][] PTestItems() =>
        new[]
        {
            new object[]{false, false, false, KnownNames.AllOn, false},
            new object[]{ true, false, false, KnownNames.AllOn, false},
            new object[]{ true,  true, false, KnownNames.AllOn, false},
            new object[]{ true,  true,  true, KnownNames.AllOn,  true},

            new object[]{false, false, false, KnownNames.AnyOn, false},
            new object[]{ true, false, false, KnownNames.AnyOn,  true},
            new object[]{ true,  true, false, KnownNames.AnyOn,  true},
            new object[]{ true,  true,  true, KnownNames.AnyOn,  true},

            new object[]{false, false, false, KnownNames.AllOff, true},
            new object[]{ true, false, false, KnownNames.AllOff, false},
            new object[]{ true,  true, false, KnownNames.AllOff, false},
            new object[]{ true,  true,  true, KnownNames.AllOff, false},

            new object[]{false, false, false, KnownNames.AnyOff, true},
            new object[]{ true, false, false, KnownNames.AnyOff, true},
            new object[]{ true,  true, false, KnownNames.AnyOff, true},
            new object[]{ true,  true,  true, KnownNames.AnyOff, false},
        }; 
    
    [Theory]
    [MemberData(nameof(PTestItems))]
    public async Task MemerContentPTest(bool v1, bool v2, bool v3, PdfName pOp, bool result)
    {
        
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new DictionaryBuilder()
                .WithItem(KnownNames.OCGs, new PdfArray(ocg1, ocg2, ocg3))
                .WithItem(KnownNames.D, new DictionaryBuilder()
                    .WithItem(KnownNames.Order, new PdfArray(ocg1,ocg2, ocg3))
                .AsDictionary())
                .AsDictionary());
        var display = await sut.ConstructUiModel(sut.Configurations[0].Order);
        display[0].Visible = v1;
        display[1].Visible = v2;
        display[2].Visible = v3;
        Assert.Equal(result, await sut.IsGroupVisible(new DictionaryBuilder()
            .WithItem(KnownNames.OCGs, new PdfArray(ocg1,ocg2,ocg3))
            .WithItem(KnownNames.Type, KnownNames.OCMD)
            .WithItem(KnownNames.P, pOp)
            .AsDictionary()));
        
    }

}