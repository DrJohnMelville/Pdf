using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects;

public class FormObjectFromPage: FormXObjectBase
{
    public FormObjectFromPage() : base("Uses a simple form xobject")
    {
    }
    
    private PdfName dictionaryName = NameDirectory.Get("GS1");
    protected override void SetPageProperties(PageCreator page)
    {
        page.AddResourceObject(ResourceTypeName.ExtGState, dictionaryName, new DictionaryBuilder()
            .WithItem(KnownNames.LW, 8).AsDictionary());
        base.SetPageProperties(page);
    }

    protected override PdfStream FormDefinition() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Form)
            .WithItem(KnownNames.BBox, new PdfArray(
                0, 0, 100, 100))
            .AsStream("/GS1 gs  0 0 m 50 50 l S");
}