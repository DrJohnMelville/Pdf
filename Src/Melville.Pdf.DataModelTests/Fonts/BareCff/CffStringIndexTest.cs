using System;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.ReferenceDocuments.Utility;

namespace Melville.Pdf.DataModelTests.Fonts.BareCff;

public class CffStringIndexTest: IDisposable
{
    public void Dispose()
    {
        source.Dispose();
    }

    private const string ThreeNameIndex = """
        0003 01 01 02 03 06 61 62 63 64 65
        """;

    private IMultiplexSource source = MultiplexSourceFactory.Create(ThreeNameIndex.BitsFromHex());

    private async ValueTask<CffIndex> NamesIndexAsync()
    {
        using var pipe = source.ParsingBufferFrom(0);
        var ret = await new CFFIndexParser(source, pipe).ParseCff1Async();
        return ret;
    }

    [Test]
    [Arguments(0, ".notdef")]
    [Arguments(1, "space")]
    [Arguments(390, "Semibold")]
    [Arguments(391, "a")]
    [Arguments(392, "b")]
    [Arguments(393, "cde")]
    public async Task ReadNameAsync(int index, string expected)
    {
        var indexSource = await NamesIndexAsync();
        var sut = new CffStringIndex(indexSource);
        (await sut.GetNameAsync(index)).Should().Be(expected);
    }
}