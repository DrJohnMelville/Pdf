using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.CmapsTest;

public class CmapGlobalParserTest
{
    [Fact]
    public async Task ParseRootCmap()
    {
        var data = """
            0000 0002
            0001 0002 0000FFFF
            0003 0005 DEADBEEF
            """.BitsFromHex();

        var cmap = await new CmapParser(MultiplexSourceFactory.Create(data)).ParseCmapTableAsync();

        cmap.Tables.Should().HaveCount(2);
        cmap.Tables[0].PlatformId.Should().Be(1);
        cmap.Tables[0].EncodingId.Should().Be(2);
        cmap.Tables[0].Offset.Should().Be(0xFFFF);
        cmap.Tables[1].PlatformId.Should().Be(3);
        cmap.Tables[1].EncodingId.Should().Be(5);
        cmap.Tables[1].Offset.Should().Be(0xDEADBEEF);
    }
}