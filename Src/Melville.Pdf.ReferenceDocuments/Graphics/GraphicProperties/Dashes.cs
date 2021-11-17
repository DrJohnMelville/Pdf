using System.Numerics;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.GraphicProperties;

public class Dashes : Card3x5
{
    public Dashes() : base("3 Horizontal Lines of Different dash styles")
    {
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.SetLineDashPattern(0, 20.0, 20.0);
        csw.SetLineWidth(5);
        HorizontalLine(csw);

        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(0, 72));
        csw.SetLineDashPattern(0, 50.0, 20.0, 10.0, 20.0);
        HorizontalLine(csw);

        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(0, 72));
        csw.SetLineDashPattern(20, 50.0, 20.0, 10.0, 20.0);
        csw.SetLineCap(LineCap.Round);
        HorizontalLine(csw);
    }

    private static void HorizontalLine(ContentStreamWriter csw)
    {
        csw.MoveTo(0.5 * 72, 0.5 * 72);
        csw.LineTo(4.5 * 72, 0.5 * 72);
        csw.StrokePath();
    }
}