using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.SfntParsers.TableDeclarations.Maximums;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt;

public class MaximumProfileReaderTest
{
    private static async Task<ParsedMaximums> ReadMaximumTableAsync(string data) =>
        await new MaxpParser(
                MultiplexSourceFactory.Create(data.BitsFromHex()).ReadPipeFrom(0))
            .ParseAsync();

    [Fact]
    public async Task ReadVersion0_5Async()
    {
        var maxp = await ReadMaximumTableAsync("""
            00005000 0033
            """);

        maxp.IsLongFormat.Should().Be(false);
        maxp.NumGlyphs.Should().Be(0x33);
    }

    [Fact]
    public async Task ReadVersion1_0Async()
    {
        var maxp = await ReadMaximumTableAsync("""
            00010000 0001 0002 0003 0004 0005 0006 0007 0008 0009 000A 000B 000C 000D 000E
            """);

        maxp.IsLongFormat.Should().Be(true);
        maxp.NumGlyphs.Should().Be(0x01);
        maxp.MaxPoints.Should().Be(0x02);
        maxp.MaxContours.Should().Be(0x03);
        maxp.MaxCompositePoints.Should().Be(0x04);
        maxp.MaxCompositeContours.Should().Be(0x05);
        maxp.MaxZones.Should().Be(0x06);
        maxp.MaxTwilightPoints.Should().Be(0x07);
        maxp.MaxStorage.Should().Be(0x08);
        maxp.MaxFunctionDefs.Should().Be(0x09);
        maxp.MaxInstructionDefs.Should().Be(0x0A);
        maxp.MaxStackElements.Should().Be(0x0B);
        maxp.MaxSizeOfInstructions.Should().Be(0x0C);
        maxp.MaxComponentElements.Should().Be(0x0D);
        maxp.MaxComponentDepth.Should().Be(0x0E);
    }

}