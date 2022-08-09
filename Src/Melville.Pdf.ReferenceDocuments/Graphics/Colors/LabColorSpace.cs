using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class LabColorSpace: ColorBars
{
    protected double MinA = -128;
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
                .WithItem(KnownNames.Range, new PdfArray(
                        new PdfDouble(MinA),new PdfDouble(127),new PdfDouble(-128),new PdfDouble(127)
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

public class LabColorSpaceClipped : LabColorSpace
{
    public LabColorSpaceClipped(): base()
    {
        MinA = 0;
    }
    //As of 12/14/2021 this case renders improper colors in the windows renderer but my output
    // matches the Adobe Reader output.
}