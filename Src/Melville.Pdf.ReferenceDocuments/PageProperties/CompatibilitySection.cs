using System.Numerics;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.ReferenceDocuments.Graphics;

namespace Melville.Pdf.ReferenceDocuments.PageProperties;

public class CompatibilitySection : Card3x5
{
    public CompatibilitySection() : base("Ignore unrecognized operators between BX and EX operators")
    {
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.SetLineWidth(10);
        csw.MoveTo(10,10);
        csw.LineTo(150,10);
        csw.StrokePath();
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(100, 100));
        using var _ = csw.BeginCompatibilitySection();
        csw.SetLineWidth(10);
        csw.MoveTo(10,10);
        csw.WriteLiteral("(This operator doesn't exist.) JDM\r\n");
        csw.LineTo(150,10);
        csw.StrokePath();
    }
}