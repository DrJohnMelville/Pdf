using System.Numerics;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public class SimpleImage: Card3x5
{
    public SimpleImage() : base("Draw a simple, generated image image")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
    }

    protected override ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(0);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(20,40));
        csw.ModifyTransformMatrix(Matrix3x2.CreateScale(250, 150));
        csw.Rectangle(0,0,1,1);
        csw.StrokePath();
        return new ValueTask();
    }
}