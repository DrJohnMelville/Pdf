using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class CalGrayGamma: ColorBars
{
    public CalGrayGamma() : base("Four different shades of the CalGray profile")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ColorSpace, PdfDirectObject.CreateName("CS1"), new PdfArray(
            KnownNames.CalGray, new DictionaryBuilder().WithItem(KnownNames.WhitePoint, new PdfArray(
                    0.9505, 1.000, 1.0890))
                .WithItem(KnownNames.Gamma, 2.5).AsDictionary()));
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        await csw.SetStrokingColorSpaceAsync(PdfDirectObject.CreateName("CS1"));
        csw.SetLineWidth(15);
        DrawLine(csw);
        csw.SetStrokeColor(0.25);
        DrawLine(csw);
        csw.SetStrokeColor(0.5);
        DrawLine(csw);
        csw.SetStrokeColor(0.75);
        DrawLine(csw);
    }

}