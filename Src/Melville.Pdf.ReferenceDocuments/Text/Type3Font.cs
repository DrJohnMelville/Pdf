namespace Melville.Pdf.ReferenceDocuments.Text;

public class Type3Font: FontDefinitionTest
{
    public Type3Font() : base("Render a type 3 font")
    {
        TextToRender = "abaabb";
    }

    protected override PdfObject CreateFont(ILowLevelDocumentCreator arg)
    {
        var triangle = arg.Add(new DictionaryBuilder().AsStream(@"
1000 0 0 0 750 750 d1
0 0 m
375 750 l
750 0 l
f
"));
        var square = arg.Add(new DictionaryBuilder().AsStream(@"
1000 0 0 0 750 750 d1
0 0 750 750 re
f"));
        var triName = NameDirectory.Get("triangle");
        var sqName = NameDirectory.Get("square");
        var chanProcs = arg.Add(new DictionaryBuilder()
            .WithItem(sqName, square)
            .WithItem(triName, triangle)
            .AsDictionary()
        );

        var encoding = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Encoding)
            .WithItem(KnownNames.Differences, new PdfArray(new PdfInteger(97), sqName, triName))
            .AsDictionary()
        );
        
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type3)
            .WithItem(KnownNames.FontBBox, new PdfArray(
                new PdfInteger(0),
                new PdfInteger(0),
                new PdfInteger(750),
                new PdfInteger(750)
            ))
            .WithItem(KnownNames.FontMatrix, new PdfArray(
                new PdfDouble(0.001),
                new PdfDouble(0),
                new PdfDouble(0),
                new PdfDouble(0.001),
                new PdfDouble(0),
                new PdfDouble(0)
            ))
            .WithItem(KnownNames.CharProcs, chanProcs)
            .WithItem(KnownNames.Encoding, encoding)
            .WithItem(KnownNames.FirstChar, new PdfInteger(97))
            .WithItem(KnownNames.LastChar, new PdfInteger(98))
            .WithItem(KnownNames.Widths, new PdfArray(new PdfInteger(1000), new PdfInteger(1000)))
            .AsDictionary();
    }
}