using System.Numerics;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.GraphicProperties;

public class LineJoin: Card3x5
{
    public LineJoin() : base("3 Corners with different line join styles.")
    {
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.ModifyTransformMatrix(Matrix3x2.CreateScale(1.5f, 1.5f));
        csw.SetLineWidth(10);
        DrawCorner(csw);
        
        csw.SetLineJoinStyle(LineJoinStyle.Round);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(0, 72f*1f));
        DrawCorner(csw);

        csw.SetLineJoinStyle(LineJoinStyle.Bevel);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(72f*1.5f, 0));
        DrawCorner(csw);
    }

    private static void DrawCorner(ContentStreamWriter csw)
    {
        csw.MoveTo(0.25 * 72, 0.25 * 72);
        csw.LineTo(0.75 * 72, 0.75 * 72);
        csw.LineTo(1.25 * 72, 0.25 * 72);
        csw.StrokePath();
    }
}