using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;
using Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt;

public class LocationTableReaderTest
{
    [Fact]
    public async Task ReadType0LocationsAsync()
    {
        // 3 hMetrics and 2 left side bearings
        var bitsFromHex = """
            0001 0002
            0003 0004
            0005 0006
            0007
            0008
            """.BitsFromHex();
        using var source = MultiplexSourceFactory.Create(bitsFromHex);
        using var pipe = source.ReadPipeFrom(0); 
        var data = await new LocationTableParser(pipe, 7, 0).ParseAsync();

        data.TotalGlyphs.Should().Be(7);
        for (uint i = 0; i < 7; i++)
        {
            data.GetLocation(i).Should().Be(new GlyphLocation((i + 1) * 2, 2));
        }
    }
    [Fact]
    public async Task ReadType1LocationsAsync()
    {
        // 3 hMetrics and 2 left side bearings
        var bitsFromHex = """
            00000002
            00000004
            00000006
            00000008
            0000000A
            0000000C
            0000000E
            00000010
            """.BitsFromHex();
        using var source = MultiplexSourceFactory.Create(bitsFromHex);
        using var pipe = source.ReadPipeFrom(0);
        var data = await new LocationTableParser(pipe, 7, 1).ParseAsync();

        data.TotalGlyphs.Should().Be(7);
        for (uint i = 0; i < 7; i++)
        {
            data.GetLocation(i).Should().Be(new GlyphLocation((i + 1) * 2, 2));
        }
    }
}