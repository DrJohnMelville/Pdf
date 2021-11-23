using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Drawing;

public class Rectangle: Card3x5
{
    public Rectangle() : base("Draw a Rectangle")
    {
        
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);
        csw.SetLineJoinStyle(LineJoinStyle.Round);
        csw.Rectangle(30, 30, 4*72, 2*72);
        csw.StrokePath();
    }
}