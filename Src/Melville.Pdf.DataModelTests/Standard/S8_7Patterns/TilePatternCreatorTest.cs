using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.Model.Creators;

namespace Melville.Pdf.DataModelTests.Standard.S8_7Patterns;

public class TilePatternCreatorTest
{
    [Test]
    [Arguments("/Type/Pattern")]
    [Arguments("/PatternType 1")]
    [Arguments("/XStep 17")]
    [Arguments("/YStep 21")]
    [Arguments("/BBox[10 20 30 40]")]
    [Arguments("/Matrix[15 25 35 45 55 65]")]
    [Arguments("1 2 3 4 re")]
    public async Task CreateTilePatternAsync(string partialString)
    {
        var builder = new TilePatternCreator(PatternPaintType.Colored, PatternTileType.Constant,
            17, 21, new PdfRect(10, 20, 30, 40));
        builder.AddMatrix(new Matrix3x2(15,25,35,45,55,65));
        await TestTilePatternAsync(partialString, builder);
    }

    private static async Task TestTilePatternAsync(string partialString, TilePatternCreator builder)
    {
        await builder.AddToContentStreamAsync(new DictionaryBuilder(), csw =>
        {
            csw.Rectangle(1, 2, 3, 4);
            return ValueTask.CompletedTask;
        });
        var (ir, num) = builder.ConstructItem(new LowLevelDocumentBuilder(), PdfDirectObject.CreateNull());
        var str = (await ir.LoadValueAsync())
            .TryGet(out PdfStream? innerStr)
            ? await innerStr.WriteStreamToStringAsync()
            : throw new InvalidOperationException("Pattern must be a stream");
        Assert.Contains(partialString, str);
    }
    [Test]
    [Arguments(PatternPaintType.Colored, "/PaintType 1")]
    [Arguments(PatternPaintType.Uncolored,"/PaintType 2")]
    public async Task TilePatternPaintTypeAsync(PatternPaintType paint, string partialString)
    {
        var builder = new TilePatternCreator(paint, PatternTileType.Constant, 1, 1, 
            new PdfRect(10, 20, 30, 40));
        await TestTilePatternAsync(partialString, builder);
    }
    [Test]
    [Arguments(PatternTileType.Constant, "/TilingType 1")]
    [Arguments(PatternTileType.NoDistortion, "/TilingType 2")]
    [Arguments(PatternTileType.FastConstant, "/TilingType 3")]
    public async Task TilePatternTileTypeAsync(PatternTileType tile, string partialString)
    {
        var builder = new TilePatternCreator(PatternPaintType.Colored, tile, 1,1,
            new PdfRect(10, 20, 30, 40));
        await TestTilePatternAsync(partialString, builder);
    }
}