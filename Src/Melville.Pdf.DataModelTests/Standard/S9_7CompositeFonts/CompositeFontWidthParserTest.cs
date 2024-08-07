﻿using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S9_7CompositeFonts;

public class CompositeFontWidthParserTest
{
    private readonly Mock<IRealizedFont> rf = new(MockBehavior.Strict);

    [Fact]
    public async Task ParseType1ItemAsync()
    {
        var sut = await new FontWidthParser(new PdfFont(new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Font)
                .WithItem(KnownNames.Subtype, KnownNames.CIDFontType2)
                .WithItem(KnownNames.W, new PdfArray(
                    4, new PdfArray(
                        500, 750, 250)))
                .AsDictionary()
            )).ParseAsync();
        Assert.Equal(500*1f/1000, sut.TryGetWidth(0x04) ?? 0,3);
        Assert.Equal(750*1f/1000, sut.TryGetWidth(0x05) ?? 0,3);
        Assert.Equal(250*1f/1000, sut.TryGetWidth(0x06) ?? 0, 3);
        Assert.Equal(1f, sut.TryGetWidth(0x07) ?? 0);
        
    }
    [Fact]
    public async Task CanFollowType1DeclAsync()
    {
        var font = new PdfFont(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.CIDFontType2)
            .WithItem(KnownNames.W, new PdfArray(
                4, new PdfArray(500),
                6,7,1233))
            .AsDictionary()
        );
        var sut = await new FontWidthParser(font).ParseAsync();
        Assert.Equal(500*1f/1000, sut.TryGetWidth(0x04) ?? 0,3);
        Assert.Equal(1000*1f/1000, sut.TryGetWidth(0x05) ?? 0,3);
        Assert.Equal(1233*1f/1000, sut.TryGetWidth(0x06) ?? 0, 3);
        Assert.Equal(1233*1f/1000, sut.TryGetWidth(0x07) ?? 0,3);
        
    }
    [Fact]
    public async Task CanFollowType2DeclAsync()
    {
        var sut = await new FontWidthParser(new PdfFont(new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Font)
                .WithItem(KnownNames.Subtype, KnownNames.CIDFontType2)
                .WithItem(KnownNames.W, new PdfArray(
                    4, 4, 500,
                    6, 7, 1233))
                .WithItem(KnownNames.DW, 34)
                .AsDictionary()
            )).ParseAsync();
        Assert.Equal(500*1f/1000, sut.TryGetWidth(0x04) ?? 0,3);
        Assert.Equal(34*1f/1000, sut.TryGetWidth(0x05) ?? 0,3);
        Assert.Equal(1233*1f/1000, sut.TryGetWidth(0x06) ?? 0, 3);
        Assert.Equal(1233*1f/1000, sut.TryGetWidth(0x07) ?? 0,3);
        
    }
    [Fact]
    public async Task ParseType2ItemAsync()
    {
        var sut = await new FontWidthParser(new PdfFont(new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Font)
                .WithItem(KnownNames.Subtype, KnownNames.CIDFontType2)
                .WithItem(KnownNames.W, new PdfArray(
                    4, 6,500))
                .AsDictionary()
            )).ParseAsync();
        Assert.Equal(500*1f/1000, sut.TryGetWidth(0x04) ?? 0);
        Assert.Equal(500*1f/1000, sut.TryGetWidth(0x05) ?? 0);
        Assert.Equal(500*1f/1000, sut.TryGetWidth(0x06) ?? 0);
        Assert.Equal(1f, sut.TryGetWidth(0x07) ?? 0);
        
    }
}