using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class CalRgb: ColorBars
{
    public CalRgb() : base("Four different Colors from RGB using the CalRgb profile")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ColorSpace, PdfDirectValue.CreateName("CS1"), new PdfValueArray(
            KnownNames.CalRGBTName, new ValueDictionaryBuilder()
                .WithItem(KnownNames.WhitePointTName, new PdfValueArray(
                0.9505, 1.000, 1.0890))
                .WithItem(KnownNames.GammaTName, new PdfValueArray(
                    1.8,1.8,1.8
                    ))
                .WithItem(KnownNames.MatrixTName, new PdfValueArray(
                    0.4497,
                    0.2446,
                    0.0252,
                    0.3163,
                    0.6720,
                    0.1412,
                    0.1854,
                    0.0833,
                    0.9227
                    ))
                .AsDictionary()));

    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpaceAsync(PdfDirectValue.CreateName("CS1"));
        DrawLine(csw);
        csw.SetStrokeColor(0.25,0,0);
        DrawLine(csw);
        csw.SetStrokeColor(0,0.5,0);
        DrawLine(csw);
        csw.SetStrokeColor(0,0,0.75);
        DrawLine(csw);
    }
}