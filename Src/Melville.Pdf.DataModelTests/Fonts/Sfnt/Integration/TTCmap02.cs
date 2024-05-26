using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts;
using Melville.Fonts.SfntParsers;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt.Integration;

public class TTCmap02
{
    private static Task<IGenericFont> loadedFont = IntegrationFontLoader.LoadAsync();

    private async Task<ICmapImplementation> LoadCmapByIndexAsync(int index) =>
        (await (await loadedFont).CmapByIndexAsync(index))!;

    [Fact]
    public async Task Type0CmapAsync()
    {
        var map = await LoadCmapByIndexAsync(1);

        map.Map(0).Should().Be(0);
        map.Map(0x1F).Should().Be(0);
        map.Map(0x20).Should().Be(3);
        map.Map(0x21).Should().Be(4);
        map.Map(0x7E).Should().Be(97);
        map.Map(0xA1).Should().Be(131);
        map.Map(0xB5).Should().Be(151);
        map.Map(0xCA).Should().Be(3);
    }

    [Fact]
    public async Task Type2CmapAsync()
    {
        var map = await LoadCmapByIndexAsync(0);

        map.Map(0).Should().Be(0);
        map.Map(0x1F).Should().Be(0);
        map.Map(0x20).Should().Be(3);
        map.Map(0x21).Should().Be(4);
        map.Map(0x7E).Should().Be(97);
        map.Map(0xA0).Should().Be(3);
        map.Map(0xAD).Should().Be(16);
        map.Map(0xB0).Should().Be(131);
        map.Map(0xB2).Should().Be(240);
        map.Map(0xB5).Should().Be(151);
        map.Map(0xD7).Should().Be(238);
        map.Map(0x37E).Should().Be(30);
    }

    [Fact]
    public async Task EnumerateType2CmapAsync()
    {
        var map = await LoadCmapByIndexAsync(0);

        var mappings = map.AllMappings().Where(i => i.Glyph > 0).ToList();
        mappings.Should().Contain((1, 0x20, 3));
        mappings.Should().Contain((1, 0x21, 4));
        mappings.Should().Contain((1, 0x7E, 97));
        mappings.Should().Contain((1, 0xA0, 3));
        mappings.Should().Contain((1, 0xAD, 16));
        mappings.Should().Contain((1, 0xB0, 131));
        mappings.Should().Contain((1, 0xB2, 240));
        mappings.Should().Contain((1, 0xB5, 151));
        mappings.Should().Contain((1, 0xD7, 238));
        mappings.Should().Contain((2, 0x37E, 30));
    }

    [Fact]
    public async Task LoadHeadTableAsync()
    {
        var head = await ((SFnt)(await loadedFont)).HeadTableAsync();

        head.MajorVersion.Should().Be(1);
        head.MinorVersion.Should().Be(0);
        head.FontRevision.ToString().Should().Be("7");
        head.CheckSumAdjustment.Should().Be(0xECA2BCD2);
        head.MagicNumber.Should().Be(0x5F0F3CF5);
        head.Flags.Should().Be((HeaderFlags)0x081b);
        head.UnitsPerEm.Should().Be(0x800);
        head.Created.Should().Be(new DateTime(1904,1,1).AddSeconds(0x00000000a2e3272a));
        head.Modified.Should().Be(new DateTime(1904,1,1).AddSeconds(0x00000000d684e4ec));
        head.XMin.Should().Be(-1361);
        head.YMin.Should().Be(-665);
        head.XMax.Should().Be(4096);
        head.YMax.Should().Be(2129);
        head.MacStyle.Should().Be(0);
        head.LowestRecPPEM.Should().Be(9);
        head.FontDirectionHint.Should().Be(1);
        head.IndexToLocFormat.Should().Be(1);
        head.GlyphDataFormat.Should().Be(0);
    }

    [Fact]
    public async Task LoadHorizontaHeaderTableAsync()
    {
        var hhea = await ((SFnt)(await loadedFont)).HorizontalHeaderTableAsync();

        hhea.MajorVersion.Should().Be(1);
        hhea.MinorVersion.Should().Be(0);
        hhea.Ascender.Should().Be(1854);
        hhea.Descender.Should().Be(-434);
        hhea.LineGap.Should().Be(67);
        hhea.AdvanceWidthMax.Should().Be(4096);
        hhea.MinLeftSideBearing.Should().Be(-1361);
        hhea.MinRightSideBearing.Should().Be(-1414);
        hhea.XMaxExtent.Should().Be(4096);
        hhea.CaretSlopeRise.Should().Be(1);
        hhea.CaretSlopeRun.Should().Be(0);
        hhea.CaretOffset.Should().Be(0);
        hhea.Reserved1.Should().Be(0);
        hhea.Reserved2.Should().Be(0);
        hhea.Reserved3.Should().Be(0);
        hhea.Reserved4.Should().Be(0);
        hhea.MetricDataFormat.Should().Be(0);
        hhea.NumberOfHMetrics.Should().Be(4502);

    }
}