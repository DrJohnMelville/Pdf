using System.Numerics;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.GraphicProperties;

public class MiterLimit: Card3x5
{
    public MiterLimit() : base("MiterLimitation.")
    {
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.ModifyTransformMatrix(Matrix3x2.CreateScale(1.5f, 1.5f));
        csw.SetLineWidth(10);
        DrawCorner(csw);
        
        csw.SetMiterLimit(2);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(0, 72f*1f));
        DrawCorner(csw);

        csw.SetMiterLimit(1);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(72f*1.5f, 0));
        DrawCorner(csw);
    }

    private static void DrawCorner(ContentStreamWriter csw)
    {
        var bottom = 0.25 * 72;
        var top = 0.75 * 72;
        csw.MoveTo(0.25 * 72, bottom);
        csw.LineTo(0.35 * 72, top);
        csw.LineTo(0.5 * 72, bottom);
        csw.LineTo(0.75 * 72, top);
        csw.LineTo(1.25 * 72, bottom);
        csw.StrokePath();
    }
}