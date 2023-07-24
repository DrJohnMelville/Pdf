using System.Numerics;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.TilingPatterns;

public class ColoredTilePattern: PatternDisplayClass
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

    protected ColoredTilePattern(string helpText) : base(helpText)
    {
    }


    protected override PdfObject CreatePattern(IPdfObjectCreatorRegistry lldc)
    {
        var tpc = CreatePatternCreator();

        tpc.AddResourceObject(ResourceTypeName.Font, PdfDirectValue.CreateName("F1"), EncodedDingbatsFont(lldc));
        tpc.AddToContentStream(new ValueDictionaryBuilder(), PatternContent());
        return tpc.ConstructItem(lldc, null).Reference;
    }

    private static PdfDictionary EncodedDingbatsFont(IPdfObjectCreatorRegistry lldc) =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type1TName)
            .WithItem(KnownNames.BaseFontTName, BuiltInFontName.ZapfDingbats)
            .WithItem(KnownNames.EncodingTName, lldc.Add(EncodeSuitesAs1To4()))
            .AsDictionary();

    private static PdfDictionary EncodeSuitesAs1To4() =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.EncodingTName)
            .WithItem(KnownNames.DifferencesTName, new PdfValueArray(
                1,
                PdfDirectValue.CreateName("a109"),
                PdfDirectValue.CreateName("a110"),
                PdfDirectValue.CreateName("a111"),
                PdfDirectValue.CreateName("a112")
            ))
            .AsDictionary();

    protected virtual TilePatternCreator CreatePatternCreator()
    {
        var tpc = new TilePatternCreator(PatternPaintType.Colored, PatternTileType.NoDistortion, 100, 100,
            new PdfRect(0, 0, 100, 100));
        tpc.AddMatrix(Matrix3x2.CreateScale(0.4f));
        return tpc;
    }
}