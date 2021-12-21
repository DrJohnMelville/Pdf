using System.Numerics;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images;

public abstract class DisplayImageTest : Card3x5
{
    protected DisplayImageTest(string helpText) : base(helpText)
    {
    }
    protected override void SetPageProperties(PageCreator page)
    {
        page.AddResourceObject(ResourceTypeName.XObject, NameDirectory.Get("I1"),
            cr=>CreateImage()
        );
    }
    
    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(0);
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(20,40));
        csw.ModifyTransformMatrix(Matrix3x2.CreateScale(250, 150));
        csw.Rectangle(0,0,1,1);
        csw.StrokePath();
        await csw.DoAsync(NameDirectory.Get("I1"));
    }

    protected abstract PdfStream CreateImage();
}