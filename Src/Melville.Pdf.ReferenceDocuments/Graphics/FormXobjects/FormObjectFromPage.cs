
namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects;

public class FormObjectFromPage: FormXObjectBase
{
    public FormObjectFromPage() : base("Uses a simple form xobject")
    {
    }
    
    private PdfDirectObject dictionaryName = PdfDirectObject.CreateName("GS1");
    protected override void SetPageProperties(PageCreator page)
    {
        page.AddResourceObject(ResourceTypeName.ExtGState, dictionaryName, new DictionaryBuilder()
            .WithItem(KnownNames.LWTName, 8).AsDictionary());
        base.SetPageProperties(page);
    }

    protected override PdfStream FormDefinition() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.FormTName)
            .WithItem(KnownNames.BBoxTName, new PdfArray(
                0, 0, 100, 100))
            .AsStream("/GS1 gs  0 0 m 50 50 l S");
}