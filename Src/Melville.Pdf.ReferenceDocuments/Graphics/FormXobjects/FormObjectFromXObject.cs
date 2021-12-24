namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects;

public class FormObjectFromXObject: FormXObjectBase
{
    public FormObjectFromXObject() : base("Uses a simple form xobject with an xobject resource")
    {
    }
    
    private PdfName dictionaryName = NameDirectory.Get("GS1");

    private static PdfDictionary SettingDict()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.LW, new PdfInteger(8)).AsDictionary();
    }

    protected override PdfStream FormDefinition() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Form)
            .WithItem(KnownNames.BBox, new PdfArray(
                new PdfInteger(0), new PdfInteger(0), new PdfInteger(100), new PdfInteger(100)))
            .WithItem(KnownNames.Resources, new DictionaryBuilder()
                .WithItem(KnownNames.ExtGState, new DictionaryBuilder()
                    .WithItem(dictionaryName, SettingDict())
                    .AsDictionary())
                .AsDictionary())
            .AsStream("/GS1 gs  0 0 m 50 50 l S");
}