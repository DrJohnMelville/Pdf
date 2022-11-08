using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.ReferenceDocuments.Graphics;

namespace Melville.Pdf.ReferenceDocuments.PageProperties;

public class SegmentedContentStream: Card3x5
{
    public SegmentedContentStream() : base("Page where contentstream is an array")
    {
    }
    
    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.CurveTo(20, 4 * 72, 4.7 * 72, 4 * 72, 4.7 * 72, 1.5 * 72);
        csw.ClosePath();
        csw.StrokePath();
    }

    protected override async ValueTask SetPagePropertiesAsync(PageCreator page)
    {
        await base.SetPagePropertiesAsync(page);
        await page.AddToContentStreamAsync(CreateContnentStream2Async);
    }

    private ValueTask CreateContnentStream2Async(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);
        csw.SetLineJoinStyle(LineJoinStyle.Round);
        csw.MoveTo(20, 1.5 * 72);
        return new ValueTask();
    }
}