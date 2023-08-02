
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
            .WithItem(KnownNames.LW, 8).AsDictionary();
    }

    protected override PdfStream FormDefinition() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Form)
            .WithItem(KnownNames.BBox, new PdfArray(
                0, 0, 100, 100))
            .WithItem(KnownNames.Resources, new DictionaryBuilder()
                .WithItem(KnownNames.ExtGState, new DictionaryBuilder()
                    .WithItem(dictionaryName, SettingDict())
                    .AsDictionary())
                .AsDictionary())
            .AsStream("/GS1 gs  0 0 m 50 50 l S");
}