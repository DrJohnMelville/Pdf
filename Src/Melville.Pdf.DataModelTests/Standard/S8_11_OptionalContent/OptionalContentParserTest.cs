using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
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
    public async Task DefaultVisibilityAsync()
    {
        
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new DictionaryBuilder()
                .WithItem(KnownNames.OCGs, new PdfArray(ocg1, ocg2))
                .AsDictionary()
            );
        
        Assert.False(await sut.IsGroupVisibleAsync(ocg1));
        Assert.False(await sut.IsGroupVisibleAsync(ocg2));
    }
    [Fact]
    public async Task AllOffVisibilityAsync()
    {
        
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new DictionaryBuilder()
                .WithItem(KnownNames.OCGs, new PdfArray(ocg1, ocg2))
                .WithItem(KnownNames.D, new DictionaryBuilder()
                    .WithItem(KnownNames.BaseState, KnownNames.OFF)
                    .AsDictionary())
                .AsDictionary()
            );
        
        Assert.False(await sut.IsGroupVisibleAsync(ocg1));
        Assert.False(await sut.IsGroupVisibleAsync(ocg2));
    }
    [Fact]
    public async Task TurnOneOnAsync()
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
        
        Assert.True(await sut.IsGroupVisibleAsync(ocg1));
        Assert.False(await sut.IsGroupVisibleAsync(ocg2));
    }

    [Fact]
    public async Task ParseSimpleOrderAsync()
    {
        var source = new DictionaryBuilder()
            .WithItem(KnownNames.OCGs, new PdfArray(ocg1, ocg2))
            .WithItem(KnownNames.D, new DictionaryBuilder()
                .WithItem(KnownNames.Order, new PdfArray(ocg2, ocg1))
                .AsDictionary())
            .AsDictionary();
        var sut = await OptionalContentPropertiesParser.ParseAsync(source);

        var display = await sut.ConstructUiModelAsync(sut.Configurations[0].Order);
        Assert.Equal("O2", display[0].Name);
        Assert.Equal("O1", display[1].Name);
        
    }
    [Fact]
    public async Task RadioButtonsAsync()
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

        var display = await sut.ConstructUiModelAsync(sut.Configurations[0].Order);
        var disp =await sut.ConstructUiModelAsync(sut.SelectedConfiguration!.Order);
        disp[0].Visible = true;
        Assert.False(disp[1].Visible);
        disp[1].Visible = true;
        Assert.False(disp[0].Visible);

        // setting to false does not reset
        disp[0].Visible = false;
        Assert.True(disp[1].Visible);

    }
    [Fact]
    public async Task ParseOrderWithTitleAsync()
    {
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new DictionaryBuilder()
                .WithItem(KnownNames.OCGs, new PdfArray(ocg1, ocg2))
                .WithItem(KnownNames.D, new DictionaryBuilder()
                    .WithItem(KnownNames.Order, new PdfArray( 
                        new PdfArray(PdfDirectObject.CreateString("Title"u8),ocg2, ocg1)))
                    .AsDictionary())
                .AsDictionary()
        );

        var display = await sut.ConstructUiModelAsync(sut.Configurations[0].Order);
        Assert.Equal("Title", display[0].Name);
        Assert.False(display[0].ShowCheck);
        
        Assert.Equal("O2", display[0].Children[0].Name);
        Assert.Equal("O1", display[0].Children[1].Name);
        
    }
    [Fact]
    public async Task ParseOrderWithSuperGroupAsync()
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

        var display = await sut.ConstructUiModelAsync(sut.Configurations[0].Order);
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
    public async Task MemerContentPTestAsync(bool v1, bool v2, bool v3, PdfDirectObject pOp, bool result)
    {
        var sut = await CreateVisContextAsync(v1, v2, v3);
        Assert.Equal(result, await sut.IsGroupVisibleAsync(new DictionaryBuilder()
            .WithItem(KnownNames.OCGs, new PdfArray(ocg1,ocg2,ocg3))
            .WithItem(KnownNames.Type, KnownNames.OCMD)
            .WithItem(KnownNames.P, pOp)
            .AsDictionary()));
    }

    private async Task<IOptionalContentState> CreateVisContextAsync(bool v1, bool v2, bool v3)
    {
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new DictionaryBuilder()
                .WithItem(KnownNames.OCGs, new PdfArray(ocg1, ocg2, ocg3))
                .WithItem(KnownNames.D, new DictionaryBuilder()
                    .WithItem(KnownNames.Order, new PdfArray(ocg1, ocg2, ocg3))
                    .AsDictionary())
                .AsDictionary());
        var display = await sut.ConstructUiModelAsync(sut.Configurations[0].Order);
        display[0].Visible = v1;
        display[1].Visible = v2;
        display[2].Visible = v3;
        return sut;
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false,  true)]
    [InlineData( true, false)]
    [InlineData( true,  true)]
    public async Task AndVETestAsync(bool a, bool b)
    {
        var sut = await CreateVisContextAsync(a, b, true);
        Assert.Equal(a & b, await sut.IsGroupVisibleAsync(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.OCMD)
            .WithItem(KnownNames.VE, new PdfArray(KnownNames.And, ocg1, ocg2))
            .AsDictionary()));
        
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false,  true)]
    [InlineData( true, false)]
    [InlineData( true,  true)]
    public async Task OrVETestAsync(bool a, bool b)
    {
        var sut = await CreateVisContextAsync(a, b, true);
        Assert.Equal(a | b, await sut.IsGroupVisibleAsync(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.OCMD)
            .WithItem(KnownNames.VE, new PdfArray(KnownNames.Or, ocg1, ocg2))
            .AsDictionary()));
        
    }
    [Theory]
    [InlineData(false, false)]
    [InlineData(false,  true)]
    [InlineData( true, false)]
    [InlineData( true,  true)]
    public async Task NotVETestAsync(bool a, bool b)
    {
        var sut = await CreateVisContextAsync(a, b, true);
        Assert.Equal(!a, await sut.IsGroupVisibleAsync(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.OCMD)
            .WithItem(KnownNames.VE, new PdfArray(KnownNames.Not, ocg1, ocg2))
            .AsDictionary()));
        
    }
    [Theory]
    [InlineData(false, false)]
    [InlineData(false,  true)]
    [InlineData( true, false)]
    [InlineData( true,  true)]
    public async Task ImplicationVETestAsync(bool a, bool b)
    {
        var sut = await CreateVisContextAsync(a, b, true);
        Assert.Equal((!a) | b, await sut.IsGroupVisibleAsync(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.OCMD)
            .WithItem(KnownNames.VE, 
                new PdfArray(KnownNames.Or, ocg2, new PdfArray(KnownNames.Not, ocg1)))
            .AsDictionary()));
        
    }
}