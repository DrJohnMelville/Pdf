using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Drawing;

[FromConstructor]
public abstract partial class ZeroLineIsDotBase: Card3x5
{

    protected override void DoPainting(ContentStreamWriter csw)
    {
        base.DoPainting(csw);
        csw.SetLineWidth(50);
        csw.SetLineCap(LineCap.Round);
        DrawDot(csw,100,75);
        csw.SetLineCap(LineCap.Butt);
        DrawDot(csw,100,175);
        csw.SetLineCap(LineCap.Square);
        DrawDot(csw,200,175);
    }

    protected abstract void DrawDot(ContentStreamWriter csw, int x, int y);
}

public class ZeroLineIsDot : ZeroLineIsDotBase
{
    public ZeroLineIsDot() : base("Render zero length lines in various line end types")
    {
    }
    protected override void DrawDot(ContentStreamWriter csw, int x, int y)
    {
        csw.MoveTo(x,y);
        csw.LineTo(x,y);
        csw.StrokePath();
    }
}

public class ClosedNullLineIsDot: ZeroLineIsDotBase
{
    public ClosedNullLineIsDot() : base("Draw a filled circle as a line of length 0")
    {
    }
    protected override void DrawDot(ContentStreamWriter csw, int x, int y)
    {
        csw.MoveTo(x,y);
        csw.ClosePath();
        csw.StrokePath();
    }
}