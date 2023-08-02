using System.Numerics;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects;

public abstract class FormXObjectBase : Card3x5
{
    private static readonly PdfDirectObject gName = PdfDirectObject.CreateName("Fx01");

    protected FormXObjectBase(string helpText) : base(helpText)
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        page.AddResourceObject(ResourceTypeName.XObject, gName, FormDefinition()
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

    protected abstract PdfStream FormDefinition();
}