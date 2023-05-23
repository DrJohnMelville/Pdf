using System.Numerics;
using System.Runtime.CompilerServices;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Drawing;

public class Clipping : Card3x5
{
    public Clipping() : base("Interaction between clipping region and transform with the state stack")
    {
        
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.SetLineWidth(1);
        Line(csw, 0);
        Line(csw, 40);
        Line(csw, 80);
        Line(csw, 190);
        csw.StrokePath();
        csw.SetNonstrokingRgbAsync(0,0,1);
        Rectangle(csw, 30);
        Rectangle(csw, 100);
        Rectangle(csw, 170);
        csw.SetNonstrokingRgbAsync(1,0,0);
        AddClip(csw);
        Rectangle(csw, 30);
        csw.SaveGraphicsState();
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(0,40));
        AddClip(csw);
        Rectangle(csw, 100);
        csw.RestoreGraphicsState();
        Rectangle(csw, 170);
    }

    private static void Line(ContentStreamWriter csw, int yVal)
    {
        csw.MoveTo(0, yVal);
        csw.LineTo(1000, yVal);
    }

    private static void AddClip(ContentStreamWriter csw)
    {
        csw.Rectangle(0, 40, 5000, 150);
        csw.ClipToPath();
        csw.EndPathWithNoOp();
    }

    private static void Rectangle(ContentStreamWriter csw, int xOffset)
    {
        csw.Rectangle(xOffset, -1000, 50, 2000);
        csw.FillPath();
    }
}
