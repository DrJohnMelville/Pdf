using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.OptionalContent;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_11_OptionalContent;

public class OptionalContentParserTest
{
    private readonly PdfValueDictionary ocg1 = OCG("O1");
    private readonly PdfValueDictionary ocg2 = OCG("O2");
    private readonly PdfValueDictionary ocg3 = OCG("O3");
    private static PdfValueDictionary OCG(string name) =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.OCGTName)
            .WithItem(KnownNames.NameTName, name)
            .AsDictionary();
    
    [Fact]
    public async Task DefaultVisibilityAsync()
    {
        
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new ValueDictionaryBuilder()
                .WithItem(KnownNames.OCGsTName, new PdfValueArray(ocg1, ocg2))
                .AsDictionary()
            );
        
        Assert.False(await sut.IsGroupVisibleAsync(ocg1));
        Assert.False(await sut.IsGroupVisibleAsync(ocg2));
    }
    [Fact]
    public async Task AllOffVisibilityAsync()
    {
        
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new ValueDictionaryBuilder()
                .WithItem(KnownNames.OCGsTName, new PdfValueArray(ocg1, ocg2))
                .WithItem(KnownNames.DTName, new ValueDictionaryBuilder()
                    .WithItem(KnownNames.BaseStateTName, KnownNames.OFFTName)
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
            new ValueDictionaryBuilder()
                .WithItem(KnownNames.OCGsTName, new PdfValueArray(ocg1, ocg2))
                .WithItem(KnownNames.DTName, new ValueDictionaryBuilder()
                    .WithItem(KnownNames.BaseStateTName, KnownNames.OFFTName)
                    .WithItem(KnownNames.ONTName, new PdfValueArray(ocg1))
                    .AsDictionary())
                .AsDictionary()
            );
        
        Assert.True(await sut.IsGroupVisibleAsync(ocg1));
        Assert.False(await sut.IsGroupVisibleAsync(ocg2));
    }

    [Fact]
    public async Task ParseSimpleOrderAsync()
    {
        var source = new ValueDictionaryBuilder()
            .WithItem(KnownNames.OCGsTName, new PdfValueArray(ocg1, ocg2))
            .WithItem(KnownNames.DTName, new ValueDictionaryBuilder()
                .WithItem(KnownNames.OrderTName, new PdfValueArray(ocg2, ocg1))
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
            new ValueDictionaryBuilder()
                .WithItem(KnownNames.OCGsTName, new PdfValueArray(ocg1, ocg2))
                .WithItem(KnownNames.DTName, new ValueDictionaryBuilder()
                    .WithItem(KnownNames.OrderTName, new PdfValueArray(ocg2, ocg1))
                    .WithItem(KnownNames.RBGroupsTName, new PdfValueArray(new PdfValueArray(ocg2, ocg1)))
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
            new ValueDictionaryBuilder()
                .WithItem(KnownNames.OCGsTName, new PdfValueArray(ocg1, ocg2))
                .WithItem(KnownNames.DTName, new ValueDictionaryBuilder()
                    .WithItem(KnownNames.OrderTName, new PdfValueArray( 
                        new PdfValueArray(PdfDirectValue.CreateString("Title"u8),ocg2, ocg1)))
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
            new ValueDictionaryBuilder()
                .WithItem(KnownNames.OCGsTName, new PdfValueArray(ocg1, ocg2,ocg3))
                .WithItem(KnownNames.DTName, new ValueDictionaryBuilder()
                    .WithItem(KnownNames.OrderTName, new PdfValueArray( 
                        new PdfValueArray(ocg3,ocg2, ocg1)))
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
            new object[]{false, false, false, KnownNames.AllOnTName, false},
            new object[]{ true, false, false, KnownNames.AllOnTName, false},
            new object[]{ true,  true, false, KnownNames.AllOnTName, false},
            new object[]{ true,  true,  true, KnownNames.AllOnTName,  true},

            new object[]{false, false, false, KnownNames.AnyOnTName, false},
            new object[]{ true, false, false, KnownNames.AnyOnTName,  true},
            new object[]{ true,  true, false, KnownNames.AnyOnTName,  true},
            new object[]{ true,  true,  true, KnownNames.AnyOnTName,  true},

            new object[]{false, false, false, KnownNames.AllOffTName, true},
            new object[]{ true, false, false, KnownNames.AllOffTName, false},
            new object[]{ true,  true, false, KnownNames.AllOffTName, false},
            new object[]{ true,  true,  true, KnownNames.AllOffTName, false},

            new object[]{false, false, false, KnownNames.AnyOffTName, true},
            new object[]{ true, false, false, KnownNames.AnyOffTName, true},
            new object[]{ true,  true, false, KnownNames.AnyOffTName, true},
            new object[]{ true,  true,  true, KnownNames.AnyOffTName, false},
        }; 
    
    [Theory]
    [MemberData(nameof(PTestItems))]
    public async Task MemerContentPTestAsync(bool v1, bool v2, bool v3, PdfDirectValue pOp, bool result)
    {
        var sut = await CreateVisContextAsync(v1, v2, v3);
        Assert.Equal(result, await sut.IsGroupVisibleAsync(new ValueDictionaryBuilder()
            .WithItem(KnownNames.OCGsTName, new PdfValueArray(ocg1,ocg2,ocg3))
            .WithItem(KnownNames.TypeTName, KnownNames.OCMDTName)
            .WithItem(KnownNames.PTName, pOp)
            .AsDictionary()));
    }

    private async Task<IOptionalContentState> CreateVisContextAsync(bool v1, bool v2, bool v3)
    {
        var sut = await OptionalContentPropertiesParser.ParseAsync(
            new ValueDictionaryBuilder()
                .WithItem(KnownNames.OCGsTName, new PdfValueArray(ocg1, ocg2, ocg3))
                .WithItem(KnownNames.DTName, new ValueDictionaryBuilder()
                    .WithItem(KnownNames.OrderTName, new PdfValueArray(ocg1, ocg2, ocg3))
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
        Assert.Equal(a & b, await sut.IsGroupVisibleAsync(new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.OCMDTName)
            .WithItem(KnownNames.VETName, new PdfValueArray(KnownNames.AndTName, ocg1, ocg2))
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
        Assert.Equal(a | b, await sut.IsGroupVisibleAsync(new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.OCMDTName)
            .WithItem(KnownNames.VETName, new PdfValueArray(KnownNames.OrTName, ocg1, ocg2))
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
        Assert.Equal(!a, await sut.IsGroupVisibleAsync(new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.OCMDTName)
            .WithItem(KnownNames.VETName, new PdfValueArray(KnownNames.NotTName, ocg1, ocg2))
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
        Assert.Equal((!a) | b, await sut.IsGroupVisibleAsync(new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.OCMDTName)
            .WithItem(KnownNames.VETName, 
                new PdfValueArray(KnownNames.OrTName, ocg2, new PdfValueArray(KnownNames.NotTName, ocg1)))
            .AsDictionary()));
        
    }
}