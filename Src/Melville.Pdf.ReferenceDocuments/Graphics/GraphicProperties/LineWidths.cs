using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.GraphicProperties;

public class LineWidths: Card3x5
{
    public LineWidths() : base("3 Horizontal Lines of Different Widths")
    {
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        HorizontalLine(csw, 2.5);
        csw.SetLineWidth(5);
        HorizontalLine(csw, 1.5);
        csw.SetLineWidth(15);
        HorizontalLine(csw, 0.5);
    }

    private static void HorizontalLine(ContentStreamWriter csw, double yPosition)
    {
        csw.MoveTo(0.5 * 72, yPosition * 72);
        csw.LineTo(4.5 * 72, yPosition * 72);
        csw.StrokePath();
    }
}