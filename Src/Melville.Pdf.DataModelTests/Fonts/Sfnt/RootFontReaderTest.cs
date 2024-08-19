using System;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts;
using Melville.Fonts.SfntParsers;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts.Sfnt;

public class RootFontReaderTest
{
    [Theory]
    [InlineData("00010000 0000 0000 0000 0000", 1)]
    [InlineData("4F54544F 0000 0000 0000 0000", 1)]
    [InlineData("74727565 0000 0000 0000 0000", 1)]
    [InlineData("74797031 0000 0000 0000 0000", 1)]
    [InlineData("65666768 0000 0000 0000 0000", 0)]
    [InlineData("74746366 0001 0000 0000 0001 0000 0010" +
                "65666768 0000 0000 0000 0000", 1)]
    [InlineData("74746366 0001 0000 0000 0002 00000014 00000020" +
                "65666768 0000 0000 0000 0000" +
                "65666768 0000 0000 0000 0000", 2)]
    public async Task ReadZeroElementFontAsync(string headderHex, int count)
    {
        using var src = MultiplexSourceFactory.Create(headderHex.BitsFromHex());
        var parsed = await RootFontParser.ParseAsync(src);
        parsed.Should().HaveCount(count);
        foreach (SFnt font in parsed)
        {
            font.Should().BeOfType<SFnt>().Which.Tables.Should().HaveCount(0);
        }
    }
}