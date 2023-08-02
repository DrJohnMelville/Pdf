namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects;

public class ResourceDictionaryInForm : FormXObjectBase
{
    public ResourceDictionaryInForm() : base("Uses a simple form xobject")
    {
    }

    protected override PdfStream FormDefinition() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.FormTName)
            .WithItem(KnownNames.ResourcesTName, new DictionaryBuilder()
                .WithItem(KnownNames.ExtGStateTName, new DictionaryBuilder()
                    .WithItem(KnownNames.AHxTName, new DictionaryBuilder()
                        .WithItem(KnownNames.LWTName, 5)
                        .WithItem(KnownNames.DTName, new PdfArray(new PdfArray(30, 10), 0))
                        .AsDictionary())
                    .AsDictionary())
                .AsDictionary()
            )
            .WithItem(KnownNames.BBoxTName, new PdfArray(
                0, 0, 100, 100))
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
        page.AddResourceObject(ResourceTypeName.ExtGState, KnownNames.AHxTName, new DictionaryBuilder()
                .WithItem(KnownNames.LWTName, 5)
                .WithItem(KnownNames.DTName, new PdfArray(new PdfArray(20, 10), 0))
                .AsDictionary());
    }

    protected override PdfStream FormDefinition() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.FormTName)
            .WithItem(KnownNames.BBoxTName, new PdfArray(
                0, 0, 100, 100))
            .AsStream("/AHx gs 0 0 m 50 50 l S");
}