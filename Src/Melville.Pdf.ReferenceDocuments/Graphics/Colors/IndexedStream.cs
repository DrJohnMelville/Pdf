using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class IndexedStream: ColorBars
{
    public IndexedStream() : base("Four different Colors from An Indexed L*A*B* ColorSpace with table in a stream")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ColorSpace, NameDirectory.Get("CS2"), new PdfArray(
            KnownNames.Lab, new DictionaryBuilder()
                .WithItem(KnownNames.WhitePoint, new PdfArray(
                    new PdfDouble(0.9505), new PdfDouble(1.000), new PdfDouble(1.0890)))
                .WithItem(KnownNames.Range, new PdfArray(
                    new PdfDouble(-128),new PdfDouble(127),new PdfDouble(-128),new PdfDouble(127)
                ))
                .AsDictionary()));        
        page.AddResourceObject(ResourceTypeName.ColorSpace, NameDirectory.Get("CS1"),
            i=>new PdfArray(
                KnownNames.Indexed, KnownNames.DeviceRGB,// NameDirectory.Get("CS2"), 
                new PdfInteger(3),
                i.Add(new DictionaryBuilder().AsStream(
                    new byte[]
                    {
                        0x80, 0x80, 0x80,
                        0x80, 0xA0, 0xA0,
                        0x80, 0x80, 0xA0,
                        0x80, 0x40, 0xA0
                    }
                    ))
            )
        );
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpace(NameDirectory.Get("CS1"));
        DrawLine(csw);
        csw.SetStrokeColor(1);
        DrawLine(csw);
        csw.SetStrokeColor(2);
        DrawLine(csw);
        csw.SetStrokeColor(3);
        DrawLine(csw);
    }
}