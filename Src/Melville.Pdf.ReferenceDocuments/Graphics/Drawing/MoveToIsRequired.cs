using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Drawing;

public class MoveToIsRequired : Card3x5
{
    public MoveToIsRequired(): base("Line to points prior to a first MoveTo do not render."){}

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.SetLineWidth(10);
        csw.LineTo(100,100);
        csw.StrokePath();
        
        csw.MoveTo(100,0);
        csw.LineTo(200,100);
        csw.StrokePath();
    }
}