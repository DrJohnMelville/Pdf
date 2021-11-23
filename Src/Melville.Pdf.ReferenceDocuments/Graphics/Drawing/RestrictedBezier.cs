using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Drawing;

public class RestrictedBezier: Card3x5
{
    public RestrictedBezier() : base("Bezier Curve using both 4 point options")
    {
        
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.MoveTo(20, 1.5 * 72);
        csw.CurveToWithoutInitialControl(2.5*72, 3*72,   4.7 * 72, 1.5 * 72);
        csw.StrokePath();
        csw.SetLineDashPattern(0, 10, 3, 5, 3);
        csw.MoveTo(20, 1.5 * 72);
        csw.CurveToWithoutFinalControl(2.5*72, 3*72,   4.7 * 72, 1.5 * 72);
        csw.StrokePath();
    }
}