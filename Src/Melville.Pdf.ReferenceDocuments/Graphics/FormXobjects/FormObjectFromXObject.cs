
namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects;

public class FormObjectFromXObject: FormXObjectBase
{
    public FormObjectFromXObject() : base("Uses a simple form xobject with an xobject resource")
    {
    }
    
    private PdfDirectValue dictionaryName = PdfDirectValue.CreateName("GS1");

    private static PdfValueDictionary SettingDict()
    {
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.LWTName, 8).AsDictionary();
    }

    protected override PdfValueStream FormDefinition() =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.FormTName)
            .WithItem(KnownNames.BBoxTName, new PdfValueArray(
                0, 0, 100, 100))
            .WithItem(KnownNames.ResourcesTName, new ValueDictionaryBuilder()
                .WithItem(KnownNames.ExtGStateTName, new ValueDictionaryBuilder()
                    .WithItem(dictionaryName, SettingDict())
                    .AsDictionary())
                .AsDictionary())
            .AsStream("/GS1 gs  0 0 m 50 50 l S");
}