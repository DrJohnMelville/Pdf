using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class IndexedByteString: ColorBars
{
    public IndexedByteString() : base("Four different Colors from An Indexed ColorSpace")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ColorSpace, PdfDirectValue.CreateName("CS1"),
            IndexedColorspace()
        );
    }

    private static PdfValueArray IndexedColorspace()
    {
        return new PdfValueArray(
            KnownNames.IndexedTName, KnownNames.DeviceRGBTName, 
            3,
            PdfDirectValue.CreateString(new byte[]
            {
                0xff, 0, 0,
                0x7f, 0x7f, 0x7f,
                0, 0, 0xFF,
                0, 0xff, 00
            })
        );
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpaceAsync(PdfDirectValue.CreateName("CS1"));
        DrawLine(csw);
        csw.SetStrokeColor(1);
        DrawLine(csw);
        csw.SetStrokeColor(2);
        DrawLine(csw);
        csw.SetStrokeColor(3);
        DrawLine(csw);
    }
}