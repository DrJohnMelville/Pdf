namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects;

public class ClipAndScale: FormXObjectBase
{
    public ClipAndScale() : base("Uses a simple form xobject")
    {
    }
    
    protected override PdfStream FormDefinition() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Form)
            .WithItem(KnownNames.BBox, new PdfArray(
                new PdfInteger(0), new PdfInteger(0), new PdfInteger(100), new PdfInteger(100)))
            .WithItem(KnownNames.Matrix, new PdfArray(
                new PdfInteger(2), new PdfInteger(0), new PdfInteger(0), new PdfInteger(3), new PdfInteger(0), new PdfInteger(0)))
            .AsStream("0 0 m 50 50 l s");
}