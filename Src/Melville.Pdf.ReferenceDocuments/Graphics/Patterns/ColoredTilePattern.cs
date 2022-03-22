using System.Numerics;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Microsoft.CodeAnalysis.CSharp;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns;

public class ColoredTileWithSpacing : ColoredTilePattern
{
    public ColoredTileWithSpacing() : base("Tile pattern with spacing")
    {
    }
    
    protected override TilePatternCreator CreatePatternCreator()
    {
        var tpc = new TilePatternCreator(PatternPaintType.Colored, PatternTileType.NoDistortion, 150, 200,
            new PdfRect(0, 0, 100, 100), NoObjectStream.Instance);
        tpc.AddMatrix(Matrix3x2.CreateScale(0.4f));
        return tpc;
    }
}

public class ColoredTileRotated : ColoredTilePattern
{
    public ColoredTileRotated(): base("Colored time patter with 45 degree rotation")
    {
    }
    
    protected override TilePatternCreator CreatePatternCreator()
    {
        var tpc = new TilePatternCreator(PatternPaintType.Colored, PatternTileType.NoDistortion, 100, 100,
            new PdfRect(0, 0, 100, 100), NoObjectStream.Instance);
        tpc.AddMatrix(Matrix3x2.CreateScale(0.4f)*Matrix3x2.CreateRotation((float)(0.4)));
        return tpc;
    }

}
public class ColoredTileMissingMatrix : ColoredTilePattern
{
    public ColoredTileMissingMatrix(): base("Colored time patter with 45 degree rotation")
    {
    }
    
    protected override TilePatternCreator CreatePatternCreator()
    {
        var tpc = new TilePatternCreator(PatternPaintType.Colored, PatternTileType.NoDistortion, 100, 100,
            new PdfRect(0, 0, 100, 100), NoObjectStream.Instance);
        return tpc;
    }

}
public class ColoredTilePattern: Card3x5
{

    protected virtual string PatternContent() => @"BT
/F1 1 Tf
64 0 0 64 7.1771 2.4414 Tm
0 Tc
0 Tw
1 0 0 rg
(\001) Tj

0.7478 -0.007 TD
0 1 0 rg
(\002) Tj

-0.7323 0.7813 TD
0 0 1 rg
(\003) Tj

0.6913 0.007 TD
0 0 0 rg
(\004) Tj
ET
";

    public ColoredTilePattern() : this("Spec example of a colored tile pattern")
    {
    }

    public ColoredTilePattern(string helpText) : base(helpText)
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        page.AddResourceObject(ResourceTypeName.Pattern, NameDirectory.Get("P1"), CreatePattern);
        page.AddResourceObject(ResourceTypeName.ColorSpace, NameDirectory.Get("Cs12"), new PdfArray(
            KnownNames.Pattern, KnownNames.DeviceRGB));
    }

    private PdfObject CreatePattern(ILowLevelDocumentCreator lldc)
    {
        var tpc = CreatePatternCreator();

        var enc = lldc.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Encoding)
            .WithItem(KnownNames.Differences, new PdfArray(
                new PdfInteger(1),
                NameDirectory.Get("a109"),
                NameDirectory.Get("a110"),
                NameDirectory.Get("a111"),
                NameDirectory.Get("a112")
                ))
            .AsDictionary());
        var zapf = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type1)
            .WithItem(KnownNames.BaseFont, BuiltInFontName.ZapfDingbats)
            .WithItem(KnownNames.Encoding, enc)
            .AsDictionary();
        
        tpc.AddResourceObject(ResourceTypeName.Font, NameDirectory.Get("F1"), zapf);
        tpc.AddToContentStream(new DictionaryBuilder(), PatternContent());

        return tpc.ConstructPageTree(lldc, null, 100).Reference;
    }

    protected virtual TilePatternCreator CreatePatternCreator()
    {
        var tpc = new TilePatternCreator(PatternPaintType.Colored, PatternTileType.NoDistortion, 100, 100,
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
        
        csw.MoveTo(224.96, 49.92);
        csw.CurveTo(224.96, 77.52, 202.56, 99.92, 174.96, 99.92);
        csw.CurveTo(147.26, 99.92, 124.96, 77.52, 124.96, 49.92);
        csw.CurveTo(124.96, 22.32, 147.36, -0.08, 174.96, -0.08);
        csw.CurveTo(202.56, -0.08, 224.96, 22.32, 224.96, 49.92);
        csw.CloseFillAndStrokePath();
        
        csw.MoveTo(87.56, 201.70);
        csw.CurveTo(63.66, 187.90, 55.46, 157.32, 69.26, 133.40);
        csw.CurveTo(83.06, 109.50, 113.66, 101.30, 137.56, 115.10);
        csw.CurveTo(161.46, 128.90, 169.66, 159.50, 155.86, 183.40);
        csw.CurveTo(142.06, 207.30, 111.46, 215.50, 87.56, 201.70);
        csw.FillAndStrokePath();
        
        csw.MoveTo(50,50);
        csw.LineTo(175, 50);
        csw.LineTo(112.5, 158.253);
        csw.CloseFillAndStrokePath();
    }
}