using System.Numerics;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.ReferenceDocuments.Graphics;

namespace Melville.Pdf.ReferenceDocuments.PageProperties.Rotation;

public abstract class RotateBase: Card3x5
{
    private readonly int rotation;
    public RotateBase(int rotation) : base($"Page with a rotation value of {rotation}")
    {
        this.rotation = rotation;
    }

    protected override void SetPageProperties(PageCreator page) => page.AddRotate(rotation);

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.ModifyTransformMatrix(new Matrix3x2(1, 0, 0, 1, 150, 0));
        csw.SetLineWidth(10);
        csw.SetLineJoinStyle(LineJoinStyle.Round);
        csw.MoveTo(0,30);
        csw.LineTo(0, 150);
        csw.LineTo(-50,100);
        csw.MoveTo(0, 30);
        csw.LineTo(0, 150);
        csw.LineTo(50, 100);
        csw.StrokePath();
    }
}