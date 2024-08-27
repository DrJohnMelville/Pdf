using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt.Cff2Outlines;

public class Cff2ParserTest
{
    public const string Cff2Data = """
        02 00 05 00 07 CF 0C 24 C3 11 9B 18 00 00 00 00
        00 26 00 01 00 00 00 0C 00 01 00 00 00 1C 00 01
        00 02 C0 00 E0 00 00 00 C0 00 C0 00 E0 00 00 00
        00 00 00 02 00 00 00 01 00 00 00 02 01 01 03 05
        20 0A 20 0A 00 00 00 01 01 01 05 F7 06 DA 12 77
        9F F8 6C 9D AE 9A F4 9A 95 9F B3 9F 8B 8B 8B 8B
        85 9A 8B 8B 97 73 8B 8B 8C 80 8B 8B 8B 8D 8B 8B
        8C 8A 8B 8B 97 17 06 FB 8E 95 86 9D 8B 8B 8D 17
        07 77 9F F8 6D 9D AD 9A F3 9A 95 9F B3 9F 08 FB
        8D 95 09 1E A0 37 5F 0C 09 8B 0C 0B C2 6E 9E 8C
        17 0A DB 57 F7 02 8C 17 0B B3 9A 77 9F 82 8A 8D
        17 0C 0C DB 95 57 F7 02 85 8B 8D 17 0C 0D F7 06
        13 00 00 00 01 01 01 1B BD BD EF 8C 10 8B 15 F8
        88 27 FB 5C 8C 10 06 F8 88 07 FC 88 EF F7 5C 8C
        10 06
        """;

    [Fact]
    public async Task ParseCff2StreamAsync()
    {
        using var multiplexSource = MultiplexSourceFactory.Create(Cff2Data.BitsFromHex());
        var table = await new Cff2GlyphSourceParser(
            multiplexSource).ParseAsync();

        table.GlyphCount.Should().Be(2);
    }
}