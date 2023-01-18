using Melville.Pdf.LowLevel.Model.Wrappers;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.TilingPatterns;

public class ColoredTileMissingMatrix : ColoredTilePattern
{
    public ColoredTileMissingMatrix(): base("Colored time patter with 45 degree rotation")
    {
    }
    
    protected override TilePatternCreator CreatePatternCreator()
    {
        var tpc = new TilePatternCreator(PatternPaintType.Colored, PatternTileType.NoDistortion, 100, 100,
            new PdfRect(0, 0, 100, 100));
        return tpc;
    }

}