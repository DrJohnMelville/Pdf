using System;
using System.Linq;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.ReferenceDocuments.Utility;
using Melville.SharpFont;
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
    public void CMapTest(int map, string hexSource, string hexDest, int expecteDConsumed)
    {
        var soura = hexSource.BitsFromHex();
        var target = new uint[10];
        var result = maps.Maps[map].GetCharacters(soura.AsMemory(), target, out var consumed);
        Assert.Equal(expecteDConsumed, consumed);
        Assert.Equal(hexDest, string.Join("",
            result.Span.ToArray().Select(i=> i.ToString("X4"))));
    }
}