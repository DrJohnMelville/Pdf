using System.Numerics;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public abstract class ColorBars : Card3x5
{
    protected ColorBars(string helpText) : base(helpText)
    {
    }

    protected void DrawLine(ContentStreamWriter csw)
    {
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(0, 50));
        csw.MoveTo(0.5 * 72, 0);
        csw.LineTo(4.5 * 72, 0);
        csw.StrokePath();
    }

}