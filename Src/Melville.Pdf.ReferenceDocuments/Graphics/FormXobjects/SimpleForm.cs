using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects;

public class SimpleForm: FormXObjectBase
{
    public SimpleForm() : base("Uses a simple form xobject")
    {
    }

    protected override PdfStream FormDefinition() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Form)
            .WithItem(KnownNames.BBox, new PdfArray(
                0, 0, 100, 100))
            .AsStream("0 0 m 50 50 l S");
}