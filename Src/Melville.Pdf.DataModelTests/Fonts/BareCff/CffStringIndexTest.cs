using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.BareCff;

public class CffStringIndexTest
{
    private const string ThreeNameIndex = """
    0003 01 01 02 03 06 61 62 63 64 65
    """;

    private async ValueTask<CffIndex> NamesIndexAsync()
    {
        var source = MultiplexSourceFactory.Create(ThreeNameIndex.BitsFromHex());
        using var pipe = source.ReadPipeFrom(0);
        var ret = await new CFFIndexParser(source, pipe).ParseCff1Async();
        return ret;
    }

    [Theory]
    [InlineData(0, ".notdef")]
    [InlineData(1, "space")]
    [InlineData(390, "Semibold")]
    [InlineData(391, "a")]
    [InlineData(392, "b")]
    [InlineData(393, "cde")]
    public async Task ReadNameAsync(int index, string expected)
    {
        var indexSource = await NamesIndexAsync();
        var sut = new CffStringIndex(indexSource);
        (await sut.GetNameAsync(index)).Should().Be(expected);
    }
}