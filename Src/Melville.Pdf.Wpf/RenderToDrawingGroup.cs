using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.Model.DocumentRenderers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf;

public class RenderToDrawingGroup
{
    public async ValueTask RenderToPngStream(DocumentRenderer doc, int page, Stream stream) =>
        await WriteToBufferStream(
                DrawingGroupToBitmap(await Render(doc, page))).CreateReader()
            .CopyToAsync(stream);

    private static RenderTargetBitmap DrawingGroupToBitmap(DrawingGroup doc)
    {
        var img = new Image() { Source = new DrawingImage(doc) };
        img.Arrange(doc.Bounds);
        var rtb = new RenderTargetBitmap((int)doc.Bounds.Width, (int)doc.Bounds.Width, 72, 72, PixelFormats.Pbgra32);
        rtb.Render(img);
        return rtb;
    }

    private static MultiBufferStream WriteToBufferStream(RenderTargetBitmap rtb)
    {
        var encoder = new JpegBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));
        var mbs = new MultiBufferStream();
        encoder.Save(mbs);
        return mbs;
    }

    public async ValueTask<DrawingGroup> Render(DocumentRenderer doc, int page)
    {
        var dg = CreateDrawingGroup();
        await RenderTo(doc, page, dg);
        dg.Freeze();
        return dg;
    }

    private static DrawingGroup CreateDrawingGroup()
    {
        var dg = new DrawingGroup();
        RenderOptions.SetBitmapScalingMode(dg, BitmapScalingMode.NearestNeighbor);
        return dg;
    }

    private async Task RenderTo(DocumentRenderer doc, int page, DrawingGroup dg)
    {
        using var dc = dg.Open();
        await RenderTo(doc, page, dc);
    }

    private  ValueTask RenderTo(DocumentRenderer doc, int page, DrawingContext dc)
    {

        AwaitConfig.ResumeOnCalledThread(true);
        return doc.RenderPageTo(page, rect =>
        {
            var innerRenderer = new WpfRenderTarget(dc);
            innerRenderer.SetBackgroundRect(rect);
            return innerRenderer;
        });
   }
}