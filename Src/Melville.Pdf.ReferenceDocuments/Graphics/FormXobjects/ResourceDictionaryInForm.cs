using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects;

public class ResourceDictionaryInForm : FormXObjectBase
{
    public ResourceDictionaryInForm() : base("Uses a simple form xobject")
    {
    }

    protected override PdfValueStream FormDefinition() =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.FormTName)
            .WithItem(KnownNames.ResourcesTName, new ValueDictionaryBuilder()
                .WithItem(KnownNames.ExtGStateTName, new ValueDictionaryBuilder()
                    .WithItem(KnownNames.AHxTName, new ValueDictionaryBuilder()
                        .WithItem(KnownNames.LWTName, 5)
                        .WithItem(KnownNames.DTName, new PdfValueArray(new PdfValueArray(30, 10), 0))
                        .AsDictionary())
                    .AsDictionary())
                .AsDictionary()
            )
            .WithItem(KnownNames.BBoxTName, new PdfValueArray(
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
        page.AddResourceObject(ResourceTypeName.ExtGState, KnownNames.AHxTName, new ValueDictionaryBuilder()
                .WithItem(KnownNames.LWTName, 5)
                .WithItem(KnownNames.DTName, new PdfValueArray(new PdfValueArray(20, 10), 0))
                .AsDictionary());
    }

    protected override PdfValueStream FormDefinition() =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.FormTName)
            .WithItem(KnownNames.BBoxTName, new PdfValueArray(
                0, 0, 100, 100))
            .AsStream("/AHx gs 0 0 m 50 50 l S");
}