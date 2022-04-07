using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Wpf.Controls;

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

    private async ValueTask RenderTo(DrawingGroup dg)
    {
        using var dc = dg.Open();
        await RenderTo(dc, dg);
    }
    

    private  ValueTask RenderTo(DrawingContext dc, DependencyObject sizeTarget)
    {
        AwaitConfig.ResumeOnCalledThread(true);
        var d2 = doc;
        return doc.RenderPageTo(page, (rect, preTransform) =>
        {
            var innerRenderer = new WpfRenderTarget(dc);
            d2.InitializeRenderTarget(innerRenderer, rect, rect.Width, rect.Height, preTransform);
            PageDisplay.SetPageSize(sizeTarget, new PageSize(rect.Width, rect.Height));
            return innerRenderer;
        });
   }

    public async ValueTask<DrawingVisual> RenderToDrawingVisual()
    {
        var ret = new DrawingVisual();
        using var dc = ret.RenderOpen();
        await RenderTo(dc, ret);
        return ret;
    }
}