using System;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt.CFFOutlines;

public class CffParserTest:IDisposable
{
    private const string CffData = """
        0100 0401 0001 0101 1341 4243 4445 462b
        5469 6d65 732d 526f 6d61 6e00 0101 011f 
        f81b 00f8 1c02 f81d 03f8 1904 1c6f 000d
        fb3c fb6e fa7c fa16 05e9 11b8 f112 0003
        0101 0813 1830 3031 2e30 3037 5469 6d65
        7320 526f 6d61 6e54 696d 6573 0000 0002
        0101 0203 0e0e 7d99 f92a 99fb 7695 f773
        8b06 f79a 93fc 7c8c 077d 99f8 5695 f75e
        9908 fb6e 8cf8 7393 f710 8b09 a70a df0b
        f78e 14                                
        """;

    private readonly IMultiplexSource source = MultiplexSourceFactory.Create(
        CffData.BitsFromHex());

    public void Dispose() => source.Dispose();

    [Fact]
    public async Task ParseCFFStreamAsync()
    {
        var table = await 
            (await new CffGlyphSourceParser(source, 1).ParseGenericFontAsync())[0]
            .GetGlyphSourceAsync();
        table.GlyphCount.Should().Be(2);
    }

    [Fact]
    public async Task ParseBareCffFormatAsync()
    {
        var gf = await RootFontParser.ParseAsync(source);
        gf.Count.Should().Be(1);
    }
}