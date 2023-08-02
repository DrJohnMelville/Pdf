namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects;

public class ClipAndScale: FormXObjectBase
{
    public ClipAndScale() : base("Uses a simple form xobject")
    {
    }
    
    protected override PdfStream FormDefinition() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.FormTName)
            .WithItem(KnownNames.BBoxTName, new PdfArray(
                0, 0, 100, 100))
            .WithItem(KnownNames.MatrixTName, new PdfArray(
                2, 0, 0, 3, 0, 0))
            .AsStream("0 0 m 50 50 l s");
}