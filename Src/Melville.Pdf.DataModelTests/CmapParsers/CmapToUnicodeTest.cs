using System;
using System.Linq;
using Melville.INPC;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.CmapParsers;

public partial class CmapToCodeTest: IClassFixture<ParsedCMaps>
{
    [FromConstructor] private readonly ParsedCMaps maps;

    [Theory]
    [InlineData(0, "10", "0000" , 1)]
    [InlineData(0, "19", "0000" , 1)]
    [InlineData(0, "20", "0001" , 1)]
    [InlineData(0, "21", "0002" , 1)]
    [InlineData(0, "30", "0011" , 1)]
    [InlineData(0, "7D", "005E" , 1)]
    [InlineData(0, "7E", "005F" , 1)]
    [InlineData(0, "7F", "000F" , 1)]
    [InlineData(0, "8147", "0000" , 2)]
    [InlineData(0, "8148", "8149" , 2)]
    [InlineData(1, "3A51", "D840DC3E" , 2)]
    [InlineData(1, "0000", "12340020" , 2)]
    [InlineData(1, "0001", "12340021" , 2)]
    [InlineData(1, "0010", "12340030" , 2)]
    [InlineData(1, "005F", "00660066" , 2)]
    [InlineData(1, "0060", "00660069" , 2)]
    [InlineData(1, "0061", "00660066006C" , 2)]
    [InlineData(1, "3A52", "0397" , 2)]
    public void CMapTest(int map, string hexSource, string hexDest, int expecteDConsumed)
    {
        var soura = hexSource.BitsFromHex();
        var target = new uint[10];
        var result = maps.Maps[map].GetCharacters(soura.AsMemory(), target, out var consumed);
        Assert.Equal(expecteDConsumed, consumed);
        Assert.Equal(hexDest, string.Join("",
            result.Span.ToArray().Select(i=> i.ToString("X4"))));
    }

    [Fact]
    public void CMapWithInsufficientBufferText()
    {
        var soura = "0061".BitsFromHex();
        var target = new uint[2];
        var result = maps.Maps[1].GetCharacters(soura.AsMemory(), target, out var consumed);
        Assert.Equal(2, consumed);
        Assert.Equal("00660066006C", string.Join("",
            result.Span.ToArray().Select(i=> i.ToString("X4"))));

    }
}