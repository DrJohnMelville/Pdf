using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Drawing;

public abstract class Clipping : Card3x5
{
    public Clipping() : base("Clip a rectangle with another.")
    {
        
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.SetLineWidth(10);
        csw.SetNonstrokingRGB(0,.5,0);
        
        csw.SaveGraphicsState();
        csw.Rectangle(0, 75, 600, 50);
        csw.ClipToPath();
        csw.EndPathWithNoOp();
        
        csw.Rectangle(25,25,150,150);
        csw.FillAndStrokePath();
        
        csw.RestoreGraphicsState();

        csw.Rectangle(200,25,150,150);
        csw.FillAndStrokePath();
    }
}
