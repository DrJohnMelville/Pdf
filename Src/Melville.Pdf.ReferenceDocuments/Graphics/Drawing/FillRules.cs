using System.Numerics;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Drawing;

public class FillRules : Card3x5
{
    public FillRules() : base("Shows winding and even-odd fill rules")
    {
        
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.SetLineWidth(5);
        csw.SetNonstrokingRgbAsync(1,1,0);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(75,100));
        MakeStar(csw);
        csw.CloseFillAndStrokePath();
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(200,0));
        MakeStar(csw);
        csw.CloseFillAndStrokePathEvenOdd();
    }

    public void MakeStar(ContentStreamWriter csw)
    {
        const double radius = 75;
        csw.MoveTo(0, - radius);
        for (int i = 1; i < 5; i++)
        {
            var angle = Math.PI * i * 4.0 / 5.0;
            csw.LineTo(radius * Math.Sin(angle), - radius * Math.Cos(angle));
        }
    }
}