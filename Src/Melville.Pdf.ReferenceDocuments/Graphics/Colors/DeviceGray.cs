using System.Numerics;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class DeviceGray: Card3x5
{
    public DeviceGray() : base("Four different shades of gray")
    {
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        
        // deviceGray should be the default
        csw.SetLineWidth(15);
        DrawLine(csw);
        csw.SetStrokeColor(0.25);
        DrawLine(csw);
        csw.SetStrokeColor(0.5);
        DrawLine(csw);
        csw.SetStrokeColor(0.75);
        DrawLine(csw);
    }

    private static void DrawLine(ContentStreamWriter csw)
    {
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(0, 50));
        csw.MoveTo(0.5 * 72, 0);
        csw.LineTo(4.5 * 72, 0);
        csw.StrokePath();
    }

}