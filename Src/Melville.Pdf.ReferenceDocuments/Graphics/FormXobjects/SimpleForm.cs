using System.Numerics;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects;

public class SimpleForm: Card3x5
{
    public SimpleForm() : base("Uses a simple form xobject")
    {
    }

    private readonly static PdfName gName = NameDirectory.Get("Fx01");

    protected override void SetPageProperties(PageCreator page)
    {
        page.AddResourceObject(ResourceTypeName.XObject, gName, new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.XObject)
            .WithItem(KnownNames.Subtype, KnownNames.Form)
            .WithItem(KnownNames.BBox, new PdfArray(
                new PdfInteger(0), new PdfInteger(0), new PdfInteger(100), new PdfInteger(100)))
            .AsStream("0 0 m 50 50 l s")
        );
    }

    protected override  async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(20,20));
        await csw.DoAsync(gName);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(100,0));
        await csw.DoAsync(gName);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(0, 75));
        await csw.DoAsync(gName);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(-100, 0));
        await csw.DoAsync(gName);
    }
}