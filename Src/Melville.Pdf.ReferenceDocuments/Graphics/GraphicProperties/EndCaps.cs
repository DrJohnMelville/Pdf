using System.Numerics;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.GraphicProperties;

public class EndCaps: Card3x5
{
    public EndCaps() : base("3 Horizontal Lines of Different Widths")
    {
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        // default should be butt caps
        csw.SetLineWidth(15);
        HorizontalLine(csw);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(0, 72));
        csw.SetLineCap(LineCap.Round);
        HorizontalLine(csw);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(0, 72));
        csw.SetLineCap(LineCap.Square);
        HorizontalLine(csw);
    }

    private static void HorizontalLine(ContentStreamWriter csw)
    {
        csw.MoveTo(0.5 * 72, 0.5 * 72);
        csw.LineTo(4.5 * 72, 0.5 * 72);
        csw.StrokePath();
    }
}