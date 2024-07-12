using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.SfntParsers.TableDeclarations.Maximums;
using Melville.Fonts.SfntParsers.TableDeclarations.PostscriptDatas;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt;

public class PostTableReaderTest
{
    private static async Task<PostscriptData> ReadPostscriptTableAsync(string data) =>
        await new PostscriptTableParser(
                MultiplexSourceFactory.Create(data.BitsFromHex()).ReadPipeFrom(0))
            .ParseAsync();

    [Fact]
    public async Task Type1TableAsync()
    {
        var table = await ReadPostscriptTableAsync("""
            00010000 
            00058000
            000A 000B
            0000000C
            00000001
            00000002
            00000003
            00000004
            """);

        VerifyHeader(table, 0x00010000);
        table.GlyphNames.Should().BeEquivalentTo(PostscriptTableParser.DefaultNames);
    }
    [Fact]
    public async Task Type20TableDefaultNamesAsync()
    {
        var table = await ReadPostscriptTableAsync("""
            00020000 
            00058000
            000A 000B
            0000000C
            00000001
            00000002
            00000003
            00000004
            0003 0001 0003 0004
            """);

        VerifyHeader(table, 0x00020000);
        table.GlyphNames.Should().BeEquivalentTo([".null","space","exclam"]);
    }
    [Fact]
    public async Task Type20CustomNameDefaultNamesAsync()
    {
        var table = await ReadPostscriptTableAsync("""
            00020000 
            00058000
            000A 000B
            0000000C
            00000001
            00000002
            00000003
            00000004
            0003 0102 0003 0103
            04 4A 6F 68 6E
            08 4D656C76696C6C65
            """);

        VerifyHeader(table, 0x00020000);
        table.GlyphNames.Should().BeEquivalentTo(["John","space","Melville"]);
    }
    [Fact]
    public async Task Type25TableAsync()
    {
        var table = await ReadPostscriptTableAsync("""
            00025000 
            00058000
            000A 000B
            0000000C
            00000001
            00000002
            00000003
            00000004
            0003 01 03 04
            """);

        VerifyHeader(table, 0x00025000);
        table.GlyphNames.Should().BeEquivalentTo([".null","space","exclam"]);
    }
    [Fact]
    public async Task Type3TableAsync()
    {
        var table = await ReadPostscriptTableAsync("""
            00030000 
            00058000
            000A 000B
            0000000C
            00000001
            00000002
            00000003
            00000004
            """);

        VerifyHeader(table, 0x00030000);
        table.GlyphNames.Should().BeEmpty();
    }

    private static void VerifyHeader(PostscriptData table, uint version)
    {
        table.Version.Should().Be(version);
        table.ItalicAngle.Should().Be(5.5f);
        table.UnderlinePosition.Should().Be(0x000A);
        table.UnderlineThickness.Should().Be(0x000B);
        table.IsFixedPitch.Should().Be(0x0000000C);
        table.MinMemType42.Should().Be(0x00000001);
        table.MaxMemType42.Should().Be(0x00000002);
        table.MinMemType1.Should().Be(0x00000003);
        table.MaxMemType1.Should().Be(0x00000004);
    }
}