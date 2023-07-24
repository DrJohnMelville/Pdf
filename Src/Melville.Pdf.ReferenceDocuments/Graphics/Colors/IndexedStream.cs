using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Objects2;
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
        page.AddResourceObject(ResourceTypeName.ColorSpace, PdfDirectValue.CreateName("CS2"), new PdfValueArray(
            KnownNames.LabTName, new ValueDictionaryBuilder()
                .WithItem(KnownNames.WhitePointTName, new PdfValueArray(
                    0.9505, 1.000, 1.0890))
                .WithItem(KnownNames.RangeTName, new PdfValueArray(
                    -128,127,-128,127
                ))
                .AsDictionary()));        
        page.AddResourceObject(ResourceTypeName.ColorSpace, PdfDirectValue.CreateName("CS1"),
            i=>new PdfValueArray(
                KnownNames.IndexedTName, KnownNames.DeviceRGBTName,// PdfDirectValue.CreateName("CS2"), 
                3,
                i.Add(new ValueDictionaryBuilder().AsStream(
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