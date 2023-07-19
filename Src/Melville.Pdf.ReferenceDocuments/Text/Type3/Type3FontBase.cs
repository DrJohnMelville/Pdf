using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text.Type3;

public abstract class Type3FontBase: FontDefinitionTest
{
    protected Type3FontBase(string name) : base(name)
    {
        TextToRender = "abaabb";
    }
    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ExtGState, NameDirectory.Get("GS1"),
            new DictionaryBuilder()
                .WithItem(KnownNames.LW, 15)
                .WithItem(KnownNames.D,
                    new PdfArray(new PdfArray(30), 0))
                .AsDictionary());
    }

    protected override PdfObject CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var triangle = arg.Add(new DictionaryBuilder().AsStream(@"
/GS1 gs
1000 0 0 0 750 750 d1
0 0 1 RG %prove color setting operations have no effect
0 0 1 rg
0 0 m
375 750 l
750 0 l
s
"));
        var square = arg.Add(new DictionaryBuilder().AsStream(@"
1000 0 0 0 750 750 d1
0 0 750 750 re
B"));
        var triName = NameDirectory.Get("triangle");
        var sqName = NameDirectory.Get("square");
        var chanProcs = arg.Add(new DictionaryBuilder()
            .WithItem(sqName, square)
            .WithItem(triName, triangle)
            .AsDictionary()
        );

        var encoding = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Encoding)
            .WithItem(KnownNames.Differences, new PdfArray(97, sqName, triName))
            .AsDictionary()
        );
        
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type3)
            .WithItem(KnownNames.FontBBox, new PdfArray(
                0,
                0,
                750,
                750
            ))
            .WithItem(KnownNames.FontMatrix, new PdfArray(
                0.001,
                0,
                0,
                0.001,
                0,
                0
            ))
            .WithItem(KnownNames.CharProcs, chanProcs)
            .WithItem(KnownNames.Encoding, encoding)
            .WithItem(KnownNames.FirstChar, 97)
            .WithItem(KnownNames.LastChar, 98)
            .WithItem(KnownNames.Widths, new PdfArray(1000, 1000))
            .AsDictionary();
    }
}