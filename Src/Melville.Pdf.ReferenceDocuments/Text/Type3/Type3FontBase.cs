
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
        page.AddResourceObject(ResourceTypeName.ExtGState, PdfDirectValue.CreateName("GS1"),
            new ValueDictionaryBuilder()
                .WithItem(KnownNames.LWTName, 15)
                .WithItem(KnownNames.DTName,
                    new PdfValueArray(new PdfValueArray(30), 0))
                .AsDictionary());
    }

    protected override PdfDirectValue CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var triangle = arg.Add(new ValueDictionaryBuilder().AsStream(@"
/GS1 gs
1000 0 0 0 750 750 d1
0 0 1 RG %prove color setting operations have no effect
0 0 1 rg
0 0 m
375 750 l
750 0 l
s
"));
        var square = arg.Add(new ValueDictionaryBuilder().AsStream(@"
1000 0 0 0 750 750 d1
0 0 750 750 re
B"));
        var triName = PdfDirectValue.CreateName("triangle");
        var sqName = PdfDirectValue.CreateName("square");
        var chanProcs = arg.Add(new ValueDictionaryBuilder()
            .WithItem(sqName, square)
            .WithItem(triName, triangle)
            .AsDictionary()
        );

        var encoding = arg.Add(new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.EncodingTName)
            .WithItem(KnownNames.DifferencesTName, new PdfValueArray(97, sqName, triName))
            .AsDictionary()
        );
        
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type3TName)
            .WithItem(KnownNames.FontBBoxTName, new PdfValueArray(
                0,
                0,
                750,
                750
            ))
            .WithItem(KnownNames.FontMatrixTName, new PdfValueArray(
                0.001,
                0,
                0,
                0.001,
                0,
                0
            ))
            .WithItem(KnownNames.CharProcsTName, chanProcs)
            .WithItem(KnownNames.EncodingTName, encoding)
            .WithItem(KnownNames.FirstCharTName, 97)
            .WithItem(KnownNames.LastCharTName, 98)
            .WithItem(KnownNames.WidthsTName, new PdfValueArray(1000, 1000))
            .AsDictionary();
    }
}