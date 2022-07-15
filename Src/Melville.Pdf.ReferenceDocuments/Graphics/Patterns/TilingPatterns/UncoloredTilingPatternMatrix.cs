using System.Numerics;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.TilingPatterns;

public class UncoloredTilingPatternMatrix : UncoloredTilingPattern
{
    public UncoloredTilingPatternMatrix():base("Uncolored tiling pattern which modified the current matrix")
    {
    }
    
    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        await csw.SetNonstrokingRGB(1.0, 1.0, 0.0);
        csw.Rectangle(25, 175, 175, -150);
        csw.FillPath();

        await csw.SetNonstrokingColorSpace(NameDirectory.Get("Cs12"));
        await csw.SetNonstrokingColorExtended(NameDirectory.Get("P1"), 0.77, 0.2, 0.0);
        
        DrawCircle(csw);
        
        csw.SaveGraphicsState();
        await csw.SetNonstrokingColorExtended(NameDirectory.Get("P1"), 0.2, 0.8, 0.4);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation((float)(224.96 - 99.92), 0));
        DrawCircle(csw);
        csw.RestoreGraphicsState();

        csw.SaveGraphicsState();
        await csw.SetNonstrokingColorExtended(NameDirectory.Get("P1"), 0.3, 0.7, 1.0);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation((float)(224.96 - 99.92)/2f, 100f));
        DrawCircle(csw);
        csw.RestoreGraphicsState();

        await csw.SetNonstrokingColorExtended(NameDirectory.Get("P1"), 0.5, 0.2, 1.0);
        csw.SaveGraphicsState();
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(50,50));
        csw.MoveTo(0,0);
        csw.LineTo(125, 0);
        csw.LineTo(62.5, 108.253);
        csw.CloseFillAndStrokePath();
        csw.RestoreGraphicsState();
    }

    private static void DrawCircle(ContentStreamWriter csw)
    {
        csw.MoveTo(99.92, 49.92);
        csw.CurveTo(99.92, 77.52, 77.52, 99.91, 49.92, 99.92);
        csw.CurveTo(22.32, 99.92, -0.08, 77.52, -0.08, 49.92);
        csw.CurveTo(-0.08, 22.32, 22.32, -0.08, 49.92, -0.08);
        csw.CurveTo(77.52, -0.08, 99.92, 22.32, 99.92, 49.92);
        csw.FillAndStrokePath();
    }
}