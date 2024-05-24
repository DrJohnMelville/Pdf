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
}