using System;
using System.Buffers;
using System.Linq;
using System.Threading.Tasks;
using Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.ReferenceDocuments.Utility;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt.Cff2Outlines;

public class FontDictSelectorTest
{
    private readonly Mock<IGlyphSubroutineExecutor>[] sel =
    [
        new(), new(), new(), new(), new()
    ];

    private readonly IGlyphSubroutineExecutor[] selectors;

    public FontDictSelectorTest()
    {
        selectors = sel.Select(i => i.Object).ToArray();
    }

    [Theory]
    [InlineData(0, 4)]
    [InlineData(1, 3)]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    [InlineData(4, 0)]
    public Task Type1FontSelectorTest(uint glyph, int index) =>
        TestSelector(glyph, index, """
        FF 00 04 03 02 01 00
        """);

    private async Task TestSelector(uint glyph, int index, string selectorCode)
    {
        var source = MultiplexSourceFactory.Create(selectorCode.BitsFromHex()
        );

        var selector = await new FontDictSelectParser(source, 1, 5).ParseAsync();
        var final = selector.GetSelector(selectors);

        await final.GetExecutor(glyph).CallAsync(10, seq => ValueTask.CompletedTask);
        sel[index].Verify(i => i.CallAsync(10,
            It.IsAny<Func<ReadOnlySequence<byte>, ValueTask>>()), Times.Once);
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    [InlineData(3, 4)]
    [InlineData(4, 4)]
    public Task Type3FontSelectorTest(uint glyph, int index) =>
        TestSelector(glyph, index, """
        FF 03 0003
        0000 02
        0002 01
        0003 04
        0005
        """);

    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    [InlineData(3, 4)]
    [InlineData(4, 4)]
    public Task Type4FontSelectorTest(uint glyph, int index) =>
        TestSelector(glyph, index, """
        FF 04 00000003
        0000 0000 0002
        0000 0002 0001
        0000 0003 0004
        0000 0005
        """);
}