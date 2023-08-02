using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Drawing;

public class GeneralBezier: Card3x5
{
    public GeneralBezier() : base("Bezier Curve using 6 point option")
    {
        
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.MoveTo(20, 1.5 * 72);
        csw.CurveTo(20, 3*72,   4.7*72, 3*72,   4.7 * 72, 1.5 * 72);
        csw.StrokePath();
        csw.MoveTo(20, 1.5 * 72);
        csw.CurveTo(20, 3*72,   4.7*72, 0,   4.7 * 72, 1.5 * 72);
        csw.StrokePath();
    }
}