using System.Numerics;
using Melville.Pdf.LowLevel.Model.Wrappers;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.TilingPatterns;

public class ColoredTileRotated : ColoredTilePattern
{
    public ColoredTileRotated(): base("Colored time patter with 45 degree rotation")
    {
    }
    
    protected override TilePatternCreator CreatePatternCreator()
    {
        var tpc = new TilePatternCreator(PatternPaintType.Colored, PatternTileType.NoDistortion, 100, 100,
            new PdfRect(0, 0, 100, 100));
        tpc.AddMatrix(Matrix3x2.CreateScale(0.4f)*Matrix3x2.CreateRotation((float)(0.4)));
        return tpc;
    }

}