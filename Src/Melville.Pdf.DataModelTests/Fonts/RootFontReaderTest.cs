using System.Threading.Tasks;
using FluentAssertions;
using Melville.Fonts;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Fonts;

public class RootFontReaderTest
{
    [Theory]
    [InlineData("00010000 0000 0000 0000 0000", 1)]
    [InlineData("4F54544F 0000 0000 0000 0000", 1)]
    [InlineData("74727565 0000 0000 0000 0000", 1)]
    [InlineData("74797031 0000 0000 0000 0000", 1)]
    [InlineData("65666768 0000 0000 0000 0000", 0)]
    [InlineData("7474636f 0001 0000 0000 0001 0000 0010" +
                "65666768 0000 0000 0000 0000", 1)]
    public async Task ReadZeroElementFontAsync(string headderHex, int count)
    {
        var src = MultiplexSourceFactory.Create(headderHex.BitsFromHex());
        var parsed = await RootFontParser.ParseAsync(src);
        parsed.Should().HaveCount(count);
    }
}