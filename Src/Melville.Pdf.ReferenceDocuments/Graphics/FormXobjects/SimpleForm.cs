namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects;

public class SimpleForm: FormXObjectBase
{
    public SimpleForm() : base("Uses a simple form xobject")
    {
    }

    protected override PdfValueStream FormDefinition() =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.FormTName)
            .WithItem(KnownNames.BBoxTName, new PdfValueArray(
                0, 0, 100, 100))
            .AsStream("0 0 m 50 50 l S");
}