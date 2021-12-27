using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.Wpf;

public static class RenderToDrawingGroup
{
    public static async ValueTask RenderToPngStream(PdfPage page, Stream stream)
    {
        var doc = await Render(page);
        var img = new Image() { Source = new DrawingImage(doc) };
        int width = (int)doc.Bounds.Width;
        int height = (int)doc.Bounds.Width;
        img.Arrange(doc.Bounds);
        var rtb = new RenderTargetBitmap(width, height, 72, 72, PixelFormats.Pbgra32);
        rtb.Render(img);
        var encoder = new JpegBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));
        var mbs = new MultiBufferStream();
        encoder.Save(mbs);
        await mbs.CreateReader().CopyToAsync(stream);
        
    }
    public static async ValueTask<DrawingGroup> Render(PdfPage page)
    {
        var dg = CreateDrawingGroup();
        await RenderTo(page, dg);
        dg.Freeze();
        return dg;
    }

    private static DrawingGroup CreateDrawingGroup()
    {
        var dg = new DrawingGroup();
        RenderOptions.SetBitmapScalingMode(dg, BitmapScalingMode.NearestNeighbor);
        return dg;
    }

    private static async Task RenderTo(PdfPage page, DrawingGroup dg)
    {
        using (var dc = dg.Open())
        {
            await RenderTo(page, dc);
        }
    }

    private static async ValueTask RenderTo(PdfPage page, DrawingContext dc)
    { 
        var rect = await page.GetBoxAsync(BoxName.CropBox);
        if (!rect.HasValue) return;
       
        var renderTarget = new WpfRenderTarget(dc, new GraphicsStateStack<GlyphTypeface>(), page);
        renderTarget.SetBackgroundRect(rect.Value);

        await page.RenderTo(renderTarget);
    }
}