using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class LabColorSpace: ColorBars
{
    public LabColorSpace() : base("Four different Colors from RGB using the CalRgb profile")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ColorSpace, NameDirectory.Get("CS1"), new PdfArray(
            KnownNames.Lab, new DictionaryBuilder()
                .WithItem(KnownNames.WhitePoint, new PdfArray(
                    new PdfDouble(0.9505), new PdfDouble(1.000), new PdfDouble(1.0890)))
                .WithItem(KnownNames.Gamma, new PdfArray(
                    new PdfDouble(1.8),new PdfDouble(1.8),new PdfDouble(1.8)
                ))
                .AsDictionary()));

    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpace(NameDirectory.Get("CS1"));
        DrawLine(csw);
        csw.SetStrokeColor(50,50,50);
        DrawLine(csw);
        csw.SetStrokeColor(50,0,50);
        DrawLine(csw);
        csw.SetStrokeColor(50,-50,50);
        DrawLine(csw);
    }
}