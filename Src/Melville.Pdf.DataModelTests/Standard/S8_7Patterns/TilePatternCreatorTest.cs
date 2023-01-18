using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.Model.Creators;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_7Patterns;

public class TilePatternCreatorTest
{
    [Theory]
    [InlineData("/Type/Pattern")]
    [InlineData("/PatternType 1")]
    [InlineData("/XStep 17")]
    [InlineData("/YStep 21")]
    [InlineData("/BBox[10 20 30 40]")]
    [InlineData("/Matrix[15 25 35 45 55 65]")]
    [InlineData("1 2 3 4 re")]
    public async Task CreateTilePattern(string partialString)
    {
        var builder = new TilePatternCreator(PatternPaintType.Colored, PatternTileType.Constant,
            17, 21, new PdfRect(10, 20, 30, 40));
        builder.AddMatrix(new Matrix3x2(15,25,35,45,55,65));
        await TestTilePattern(partialString, builder);
    }

    private static async Task TestTilePattern(string partialString, TilePatternCreator builder)
    {
        await builder.AddToContentStreamAsync(new DictionaryBuilder(), csw =>
        {
            csw.Rectangle(1, 2, 3, 4);
            return ValueTask.CompletedTask;
        });
        var (ir, num) = builder.ConstructItem(new LowLevelDocumentBuilder(), null);
        var str = await (await ir.DirectValueAsync()).WriteToStringAsync();
        Assert.Contains(partialString, str);
    }
    [Theory]
    [InlineData(PatternPaintType.Colored, "/PaintType 1")]
    [InlineData(PatternPaintType.Uncolored,"/PaintType 2")]
    public async Task TilePatternPaintType(PatternPaintType paint, string partialString)
    {
        var builder = new TilePatternCreator(paint, PatternTileType.Constant, 1, 1, 
            new PdfRect(10, 20, 30, 40));
        await TestTilePattern(partialString, builder);
    }
    [Theory]
    [InlineData(PatternTileType.Constant, "/TilingType 1")]
    [InlineData(PatternTileType.NoDistortion, "/TilingType 2")]
    [InlineData(PatternTileType.FastConstant, "/TilingType 3")]
    public async Task TilePatternTileType(PatternTileType tile, string partialString)
    {
        var builder = new TilePatternCreator(PatternPaintType.Colored, tile, 1,1,
            new PdfRect(10, 20, 30, 40));
        await TestTilePattern(partialString, builder);
    }
}