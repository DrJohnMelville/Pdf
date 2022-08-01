using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Drawing;

public class WpfLineRenderBug : Card3x5
{
    public WpfLineRenderBug(): base("Shows a bug rendering a closed path with one line."){}

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.SetLineWidth(40);
        csw.MoveTo(100,50);
        csw.LineTo(100,150);
        csw.ClosePath();
        csw.StrokePath();
    }
}