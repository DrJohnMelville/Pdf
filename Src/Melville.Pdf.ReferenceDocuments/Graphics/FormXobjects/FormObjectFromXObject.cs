
namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects;

public class FormObjectFromXObject: FormXObjectBase
{
    public FormObjectFromXObject() : base("Uses a simple form xobject with an xobject resource")
    {
    }
    
    private PdfDirectObject dictionaryName = PdfDirectObject.CreateName("GS1");

    private static PdfDictionary SettingDict()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.LWTName, 8).AsDictionary();
    }

    protected override PdfStream FormDefinition() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.FormTName)
            .WithItem(KnownNames.BBoxTName, new PdfArray(
                0, 0, 100, 100))
            .WithItem(KnownNames.ResourcesTName, new DictionaryBuilder()
                .WithItem(KnownNames.ExtGStateTName, new DictionaryBuilder()
                    .WithItem(dictionaryName, SettingDict())
                    .AsDictionary())
                .AsDictionary())
            .AsStream("/GS1 gs  0 0 m 50 50 l S");
}