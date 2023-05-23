using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Drawing;

public class StrokeAndFill: Card3x5
{
    public StrokeAndFill() : base("All Combinations of Stroke and Fill")
    {
        
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.SetLineWidth(10);
        csw.SetNonstrokingRgbAsync(1,0,0);
        csw.Rectangle(20, 20, 50, 50);
        csw.StrokePath();
        csw.Rectangle(20, 120, 50, 50);
        csw.FillAndStrokePath();
 
        csw.Rectangle(120, 20, 50, 50);
        csw.EndPathWithNoOp();
        csw.Rectangle(120, 120, 50, 50);
        csw.FillPath();
    }
}