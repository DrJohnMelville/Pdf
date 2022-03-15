using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.Model.Renderers;

namespace Melville.Pdf.Wpf.Rendering;

public readonly struct RenderToDrawingGroup
{
    private readonly DocumentRenderer doc;
    private readonly int page;

    public RenderToDrawingGroup(DocumentRenderer doc, int page)
    {
        this.doc = doc;
        this.page = page;
    }

    public async ValueTask RenderToPngStream(Stream stream) =>
        await WriteToBufferStream(
                DrawingGroupToBitmap(await Render())).CreateReader()
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

    public async ValueTask<DrawingImage> RenderToDrawingImage()
    {
        var image = new DrawingImage(await Render());
        image.Freeze();
        return image;
    }

    public async ValueTask<DrawingGroup> Render()
    {
        var dg = CreateDrawingGroup();
        await RenderTo(dg);
        dg.Freeze();
        return dg;
    }

    private static DrawingGroup CreateDrawingGroup()
    {
        var dg = new DrawingGroup();
        RenderOptions.SetBitmapScalingMode(dg, BitmapScalingMode.NearestNeighbor);
        return dg;
    }

    private async Task RenderTo(DrawingGroup dg)
    {
        using var dc = dg.Open();
        await RenderTo(dc);
    }

    private  ValueTask RenderTo(DrawingContext dc)
    {

        AwaitConfig.ResumeOnCalledThread(true);
        return doc.RenderPageTo(page, (rect, preTransform) =>
        {
            var innerRenderer = new WpfRenderTarget(dc);
            innerRenderer.SetBackgroundRect(rect, preTransform);
            return innerRenderer;
        });
   }
}