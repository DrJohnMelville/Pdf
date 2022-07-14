using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.Model.Creators;

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

public class UncoloredTilingPattern : ColoredTilePattern
{
    protected UncoloredTilingPattern(string helpText) : base(helpText)
    {
    }

    public UncoloredTilingPattern():this("Uncolored tiling pattern example")
    {
    }

    protected override string PatternContent() => @"BT
/F1 1 Tf
64 0 0 64 7.1771 2.4414 Tm
0 Tc
0 Tw
(\001) Tj

0.7478 -0.007 TD
(\002) Tj

-0.7323 0.7813 TD
(\003) Tj

0.6913 0.007 TD
(\004) Tj
ET
";
    protected override TilePatternCreator CreatePatternCreator()
    {
        var tpc = new TilePatternCreator(PatternPaintType.Uncolored, PatternTileType.NoDistortion, 100, 100,
            new PdfRect(0, 0, 100, 100), NoObjectStream.Instance);
        tpc.AddMatrix(Matrix3x2.CreateScale(0.4f));
        return tpc;
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        await csw.SetNonstrokingRGB(1.0, 1.0, 0.0);
        csw.Rectangle(25, 175, 175, -150);
        csw.FillPath();

        await csw.SetNonstrokingColorSpace(NameDirectory.Get("Cs12"));
        await csw.SetNonstrokingColorExtended(NameDirectory.Get("P1"), 0.77, 0.2, 0.0);
        
        csw.MoveTo(99.92, 49.92);
        csw.CurveTo(99.92, 77.52, 77.52, 99.91, 49.92, 99.92);
        csw.CurveTo(22.32, 99.92, -0.08, 77.52, -0.08, 49.92);
        csw.CurveTo(-0.08, 22.32, 22.32, -0.08, 49.92, -0.08);
        csw.CurveTo(77.52, -0.08, 99.92, 22.32, 99.92, 49.92);
        csw.FillAndStrokePath();
        
        await csw.SetNonstrokingColorExtended(NameDirectory.Get("P1"), 0.2, 0.8, 0.4);
        csw.MoveTo(224.96, 49.92);
        csw.CurveTo(224.96, 77.52, 202.56, 99.92, 174.96, 99.92);
        csw.CurveTo(147.26, 99.92, 124.96, 77.52, 124.96, 49.92);
        csw.CurveTo(124.96, 22.32, 147.36, -0.08, 174.96, -0.08);
        csw.CurveTo(202.56, -0.08, 224.96, 22.32, 224.96, 49.92);
        csw.CloseFillAndStrokePath();
        
        await csw.SetNonstrokingColorExtended(NameDirectory.Get("P1"), 0.3, 0.7, 1.0);
        csw.MoveTo(87.56, 201.70);
        csw.CurveTo(63.66, 187.90, 55.46, 157.32, 69.26, 133.40);
        csw.CurveTo(83.06, 109.50, 113.66, 101.30, 137.56, 115.10);
        csw.CurveTo(161.46, 128.90, 169.66, 159.50, 155.86, 183.40);
        csw.CurveTo(142.06, 207.30, 111.46, 215.50, 87.56, 201.70);
        csw.FillAndStrokePath();
        
        await csw.SetNonstrokingColorExtended(NameDirectory.Get("P1"), 0.5, 0.2, 1.0);
        csw.MoveTo(50,50);
        csw.LineTo(175, 50);
        csw.LineTo(112.5, 158.253);
        csw.CloseFillAndStrokePath();
    }
}