using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Drawing;

public class ClosedPath: Card3x5
{
    public ClosedPath() : base("Stroke a path which requires a line to close it.")
    {
        
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);
        csw.SetLineJoinStyle(LineJoinStyle.Round);
        csw.MoveTo(20, 1.5 * 72);
        csw.CurveTo(20, 4*72,   4.7*72, 4*72,   4.7 * 72, 1.5 * 72);
        csw.ClosePath();
        csw.StrokePath();
    }
}