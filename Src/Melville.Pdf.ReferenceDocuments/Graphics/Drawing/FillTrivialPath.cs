using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Drawing;

public class FillTrivialPath : Card3x5
{
    public FillTrivialPath():base("A terminal Moveto is ignroed in filling"){}

    // Pdf Spec 8.5.3.3.1 
    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.MoveTo(50,50);
        csw.LineTo(100,200);
        csw.LineTo(150,50);
        csw.MoveTo(200,200);
        csw.FillPath();
    }
}