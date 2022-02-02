using System.Numerics;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.ReferenceDocuments.Graphics;

namespace Melville.Pdf.ReferenceDocuments.PageProperties;

public abstract class Rotate: Card3x5
{
    private readonly int rotation;
    public Rotate(int rotation) : base($"Page with a rotation value of {rotation}")
    {
        this.rotation = rotation;
    }

    protected override void SetPageProperties(PageCreator page) => page.AddRotate(rotation);

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.ModifyTransformMatrix(new Matrix3x2(1, 0, 0, 1, 150, 0));
        csw.SetLineWidth(10);
        csw.SetLineJoinStyle(LineJoinStyle.Round);
        csw.MoveTo(0,30);
        csw.LineTo(0, 150);
        csw.LineTo(-50,100);
        csw.MoveTo(0, 30);
        csw.LineTo(0, 150);
        csw.LineTo(50, 100);
        csw.StrokePath();
    }
}

public class Rotate0: Rotate
{
    public Rotate0() : base(0)
    {
    }
}

public class Rotate90: Rotate
{
    public Rotate90() : base(90)
    {
    }
}

public class Rotate180: Rotate
{
    public Rotate180() : base(180)
    {
    }
}

public class Rotate270: Rotate
{
    public Rotate270() : base(270)
    {
    }
}
