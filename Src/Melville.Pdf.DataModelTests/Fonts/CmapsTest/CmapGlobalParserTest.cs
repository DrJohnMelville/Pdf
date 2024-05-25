using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Linq;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.CmapsTest;

public class CmapGlobalParserTest
{
    [Fact]
    public async Task ParseRootCmapAsync()
    {
        var cmap = await ParseCmapAsync("""
            0000 0002
            0001 0002 0000FFFF
            0003 0005 DEADBEEF
            """);

        cmap.Tables.Should().HaveCount(2);
        cmap.Tables[0].PlatformId.Should().Be(1);
        cmap.Tables[0].EncodingId.Should().Be(2);
        cmap.Tables[0].Offset.Should().Be(0xFFFF);
        cmap.Tables[1].PlatformId.Should().Be(3);
        cmap.Tables[1].EncodingId.Should().Be(5);
        cmap.Tables[1].Offset.Should().Be(0xDEADBEEF);
    }

    private static async ValueTask<ParsedCmap> ParseCmapAsync(string data) => 
        (ParsedCmap) await
        new CmapParser(MultiplexSourceFactory.Create(data.BitsFromHex())).ParseCmapTableAsync();

    [Fact]
    public async Task ParseType1CmapAsync()
    {
        var select = "0000 0001"+
                      "0000 0001 0000000C" + // headder
                      "0000 0106 0000 " +
                      Enumerable.Range(0,256)
                          .Select(i=>i%2 ==0?i:(2*i)+1)
                          .Select(i=> $"{(byte)i:X2}").ConcatenateStrings();
        var cmap = await ParseCmapAsync(select);

        var subTable = await cmap.GetSubtableAsync(cmap.Tables[0]);

        subTable.Map(0).Should().Be(0);
        subTable.Map(1).Should().Be(3);
        subTable.Map(2).Should().Be(2);
        subTable.Map(3).Should().Be(7);
        subTable.TryMap(1, 5, out var retVal).Should().Be(true);
        retVal.Should().Be(11);

        subTable.AllMappings().Take(5).Should().BeEquivalentTo(new[]
        {
            (1, 0, 0),
            (1, 1, 3),
            (1, 2, 2),
            (1, 3, 7),
            (1, 4, 4),
        });
    }

    [Fact]
    public async Task ParstType10CmapAsync()
    {
        var select = "0000 0001"+
                     "0000 0001 0000000C" + // headder
                     "000A 0000 00000020 00000000 00000020 00000003 0003 0004 0005 ";
        var cmap = await ParseCmapAsync(select);
        var subtable = await cmap.GetSubtableAsync(cmap.Tables[0]);
        subtable.Map(0x20).Should().Be(3);
        subtable.Map(0x21).Should().Be(4);
        subtable.Map(0x22).Should().Be(5);

        subtable.AllMappings().Should().BeEquivalentTo([
            (4, 0x20, 3),
            (4, 0x21, 4),
            (4, 0x22, 5)
        ]);
    }

    [Fact]
    public async Task ParstType12CmapAsync()
    {
        var select = "0000 0001" +
                     "0000 0001 0000000C" + // headder
                     "000C 0000 DEADBEEF 00000000 00000005 " +
                     "00000020 00000022 00000003" +
                     "00000040 00000042 00000013" +
                     "00000050 00000052 00000023" +
                     "00000060 00000062 00000033" +
                     "00000070 00000072 00000043";
        var cmap = await ParseCmapAsync(select);
        var subtable = await cmap.GetSubtableAsync(cmap.Tables[0]);
        subtable.Map(0x10).Should().Be(0);
        subtable.Map(0x19).Should().Be(0);
        subtable.Map(0x20).Should().Be(3);
        subtable.Map(0x21).Should().Be(4);
        subtable.Map(0x22).Should().Be(5);
        subtable.Map(0x23).Should().Be(0);
        subtable.Map(0x31).Should().Be(0);
        subtable.Map(0x3F).Should().Be(0);
        subtable.Map(0x40).Should().Be(0x13);
        subtable.Map(0x41).Should().Be(0x14);
        subtable.Map(0x42).Should().Be(0x15);
        subtable.Map(0x43).Should().Be(0x0);
        subtable.Map(0x4A).Should().Be(0x0);
        subtable.Map(0x4F).Should().Be(0x0);
        subtable.Map(0x50).Should().Be(0x23);
        subtable.Map(0x51).Should().Be(0x24);
        subtable.Map(0x52).Should().Be(0x25);
        subtable.Map(0x60).Should().Be(0x33);
        subtable.Map(0x61).Should().Be(0x34);
        subtable.Map(0x62).Should().Be(0x35);
        subtable.Map(0x70).Should().Be(0x43);
        subtable.Map(0x71).Should().Be(0x44);
        subtable.Map(0x72).Should().Be(0x45);
        subtable.Map(0x73).Should().Be(0x0);
        subtable.Map(0x83).Should().Be(0x0);


        subtable.AllMappings().Should().BeEquivalentTo([
            (4, 0x20, 3),
            (4, 0x21, 4),
            (4, 0x22, 5),
            (4, 0x40, 0x13),
            (4, 0x41, 0x14),
            (4, 0x42, 0x15),
            (4, 0x50, 0x23),
            (4, 0x51, 0x24),
            (4, 0x52, 0x25),
            (4, 0x60, 0x33),
            (4, 0x61, 0x34),
            (4, 0x62, 0x35),
            (4, 0x70, 0x43),
            (4, 0x71, 0x44),
            (4, 0x72, 0x45),
        ]);
    }

    [Fact]
    public async Task ParstType13CmapAsync()
    {
        var select = "0000 0001" +
                     "0000 0001 0000000C" + // headder
                     "000D 0000 DEADBEEF 00000000 00000005 " +
                     "00000020 00000022 00000003" +
                     "00000040 00000042 00000013" +
                     "00000050 00000052 00000023" +
                     "00000060 00000062 00000033" +
                     "00000070 00000072 00000043";
        var cmap = await ParseCmapAsync(select);
        var subtable = await cmap.GetSubtableAsync(cmap.Tables[0]);
        subtable.Map(0x10).Should().Be(0);
        subtable.Map(0x19).Should().Be(0);
        subtable.Map(0x20).Should().Be(3);
        subtable.Map(0x21).Should().Be(3);
        subtable.Map(0x22).Should().Be(3);
        subtable.Map(0x23).Should().Be(0);
        subtable.Map(0x31).Should().Be(0);
        subtable.Map(0x3F).Should().Be(0);
        subtable.Map(0x40).Should().Be(0x13);
        subtable.Map(0x41).Should().Be(0x13);
        subtable.Map(0x42).Should().Be(0x13);
        subtable.Map(0x43).Should().Be(0x0);
        subtable.Map(0x4A).Should().Be(0x0);
        subtable.Map(0x4F).Should().Be(0x0);
        subtable.Map(0x50).Should().Be(0x23);
        subtable.Map(0x51).Should().Be(0x23);
        subtable.Map(0x52).Should().Be(0x23);
        subtable.Map(0x60).Should().Be(0x33);
        subtable.Map(0x61).Should().Be(0x33);
        subtable.Map(0x62).Should().Be(0x33);
        subtable.Map(0x70).Should().Be(0x43);
        subtable.Map(0x71).Should().Be(0x43);
        subtable.Map(0x72).Should().Be(0x43);
        subtable.Map(0x73).Should().Be(0x0);
        subtable.Map(0x83).Should().Be(0x0);


        subtable.AllMappings().Should().BeEquivalentTo([
            (4, 0x20, 3),
            (4, 0x21, 3),
            (4, 0x22, 3),
            (4, 0x40, 0x13),
            (4, 0x41, 0x13),
            (4, 0x42, 0x13),
            (4, 0x50, 0x23),
            (4, 0x51, 0x23),
            (4, 0x52, 0x23),
            (4, 0x60, 0x33),
            (4, 0x61, 0x33),
            (4, 0x62, 0x33),
            (4, 0x70, 0x43),
            (4, 0x71, 0x43),
            (4, 0x72, 0x43),
        ]);
    }
}