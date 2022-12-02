using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects;

public class ResourceDictionaryInForm : FormXObjectBase
{
    public ResourceDictionaryInForm() : base("Uses a simple form xobject")
    {
    }

    protected override PdfStream FormDefinition() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Form)
            .WithItem(KnownNames.Resources, new DictionaryBuilder()
                .WithItem(KnownNames.ExtGState, new DictionaryBuilder()
                    .WithItem(KnownNames.AHx, new DictionaryBuilder()
                        .WithItem(KnownNames.LW, 5)
                        .WithItem(KnownNames.D, new PdfArray(new PdfArray(30, 10), new PdfInteger(0)))
                        .AsDictionary())
                    .AsDictionary())
                .AsDictionary()
            )
            .WithItem(KnownNames.BBox, new PdfArray(
                new PdfInteger(0), new PdfInteger(0), new PdfInteger(100), new PdfInteger(100)))
            .AsStream("/AHx gs 0 0 m 50 50 l S");
}

public class ResourceDictionaryInPage : FormXObjectBase
{
    public ResourceDictionaryInPage() : base("Uses a simple form xobject")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ExtGState, KnownNames.AHx, new DictionaryBuilder()
                .WithItem(KnownNames.LW, 5)
                .WithItem(KnownNames.D, new PdfArray(new PdfArray(20, 10), new PdfInteger(0)))
                .AsDictionary());
    }

    protected override PdfStream FormDefinition() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Form)
            .WithItem(KnownNames.BBox, new PdfArray(
                new PdfInteger(0), new PdfInteger(0), new PdfInteger(100), new PdfInteger(100)))
            .AsStream("/AHx gs 0 0 m 50 50 l S");
}