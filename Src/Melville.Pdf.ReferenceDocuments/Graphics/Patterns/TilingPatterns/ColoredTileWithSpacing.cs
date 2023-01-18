using System.Numerics;
using Melville.Pdf.LowLevel.Model.Wrappers;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.TilingPatterns;

public class ColoredTileWithSpacing : ColoredTilePattern
{
    public ColoredTileWithSpacing() : base("Tile pattern with spacing")
    {
    }
    
    protected override TilePatternCreator CreatePatternCreator()
    {
        var tpc = new TilePatternCreator(PatternPaintType.Colored, PatternTileType.NoDistortion, 150, 200,
            new PdfRect(0, 0, 100, 100));
        tpc.AddMatrix(Matrix3x2.CreateScale(0.4f));
        return tpc;
    }
}