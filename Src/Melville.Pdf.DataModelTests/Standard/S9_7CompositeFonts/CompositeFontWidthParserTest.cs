using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S9_7CompositeFonts;

public class CompositeFontWidthParserTest
{
    private readonly Mock<IRealizedFont> rf = new(MockBehavior.Strict);

    [Fact]
    public async Task ParseType1Item()
    {
        var sut = await new FontWidthParser(rf.Object,
            new PdfFont(new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Font)
                .WithItem(KnownNames.Subtype, KnownNames.CIDFontType2)
                .WithItem(KnownNames.W, new PdfArray(
                    new PdfInteger(4), new PdfArray(
                        new PdfInteger(500), new PdfInteger(750), new PdfInteger(250))))
                .AsDictionary()
            ), 17).Parse(KnownNameKeys.CIDFontType2);
        Assert.Equal(500*17f/1000, sut.AdjustWidth(0x04, 0),3);
        Assert.Equal(750*17f/1000, sut.AdjustWidth(0x05, 0),3);
        Assert.Equal(250*17f/1000, sut.AdjustWidth(0x06, 0), 3);
        Assert.Equal(17f, sut.AdjustWidth(0x07, 0));
        
    }
    [Fact]
    public async Task CanFollowType1Decl()
    {
        var sut = await new FontWidthParser(rf.Object,
            new PdfFont(new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Font)
                .WithItem(KnownNames.Subtype, KnownNames.CIDFontType2)
                .WithItem(KnownNames.W, new PdfArray(
                    new PdfInteger(4), new PdfArray(
                        new PdfInteger(500)),
                    new PdfInteger(6), new PdfInteger(7), new PdfDouble(1233)))
                .AsDictionary()
            ), 17).Parse(KnownNameKeys.CIDFontType2);
        Assert.Equal(500*17f/1000, sut.AdjustWidth(0x04, 0),3);
        Assert.Equal(1000*17f/1000, sut.AdjustWidth(0x05, 0),3);
        Assert.Equal(1233*17f/1000, sut.AdjustWidth(0x06, 0), 3);
        Assert.Equal(1233*17f/1000, sut.AdjustWidth(0x07, 0),3);
        
    }
    [Fact]
    public async Task CanFollowType2Decl()
    {
        var sut = await new FontWidthParser(rf.Object,
            new PdfFont(new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Font)
                .WithItem(KnownNames.Subtype, KnownNames.CIDFontType2)
                .WithItem(KnownNames.W, new PdfArray(
                    new PdfInteger(4), new PdfInteger(4), new PdfInteger(500),
                    new PdfInteger(6), new PdfInteger(7), new PdfDouble(1233)))
                .WithItem(KnownNames.DW, 34)
                .AsDictionary()
            ), 17).Parse(KnownNameKeys.CIDFontType2);
        Assert.Equal(500*17f/1000, sut.AdjustWidth(0x04, 0),3);
        Assert.Equal(34*17f/1000, sut.AdjustWidth(0x05, 0),3);
        Assert.Equal(1233*17f/1000, sut.AdjustWidth(0x06, 0), 3);
        Assert.Equal(1233*17f/1000, sut.AdjustWidth(0x07, 0),3);
        
    }
    [Fact]
    public async Task ParseType2Item()
    {
        var sut = await new FontWidthParser(rf.Object,
            new PdfFont(new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Font)
                .WithItem(KnownNames.Subtype, KnownNames.CIDFontType2)
                .WithItem(KnownNames.W, new PdfArray(
                    new PdfInteger(4), new PdfInteger(6),new PdfInteger(500)))
                .AsDictionary()
            ), 17).Parse(KnownNameKeys.CIDFontType2);
        Assert.Equal(500*17f/1000, sut.AdjustWidth(0x04, 0));
        Assert.Equal(500*17f/1000, sut.AdjustWidth(0x05, 0));
        Assert.Equal(500*17f/1000, sut.AdjustWidth(0x06, 0));
        Assert.Equal(17f, sut.AdjustWidth(0x07, 0));
        
    }
}