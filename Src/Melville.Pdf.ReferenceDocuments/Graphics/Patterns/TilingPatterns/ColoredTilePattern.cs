using System.Numerics;
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


    protected override PdfObject CreatePattern(ILowLevelDocumentBuilder lldc)
    {
        var tpc = CreatePatternCreator();

        tpc.AddResourceObject(ResourceTypeName.Font, NameDirectory.Get("F1"), EncodedDingbatsFont(lldc));
        tpc.AddToContentStream(new DictionaryBuilder(), PatternContent());
        return tpc.ConstructPageTree(lldc, null, 100).Reference;
    }

    private static PdfDictionary EncodedDingbatsFont(ILowLevelDocumentBuilder lldc) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type1)
            .WithItem(KnownNames.BaseFont, BuiltInFontName.ZapfDingbats)
            .WithItem(KnownNames.Encoding, lldc.Add(EncodeSuitesAs1To4()))
            .AsDictionary();

    private static PdfDictionary EncodeSuitesAs1To4() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Encoding)
            .WithItem(KnownNames.Differences, new PdfArray(
                1,
                NameDirectory.Get("a109"),
                NameDirectory.Get("a110"),
                NameDirectory.Get("a111"),
                NameDirectory.Get("a112")
            ))
            .AsDictionary();

    protected virtual TilePatternCreator CreatePatternCreator()
    {
        var tpc = new TilePatternCreator(PatternPaintType.Colored, PatternTileType.NoDistortion, 100, 100,
            new PdfRect(0, 0, 100, 100), NoObjectStream.Instance);
        tpc.AddMatrix(Matrix3x2.CreateScale(0.4f));
        return tpc;
    }
}