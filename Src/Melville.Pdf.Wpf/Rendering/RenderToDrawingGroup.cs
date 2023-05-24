using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.Wpf.Rendering;

/// <summary>
/// Facade object thjat handles rendering PDF files to various WPF objects.
///
/// Most of the methods on this class eventually call RenderToDrawingVisual to do the actual painting
/// </summary>
public readonly struct RenderToDrawingGroup
{
    private readonly DocumentRenderer doc;
    private readonly int oneBasedPageNumber;

    /// <summary>
    /// Create a RenderToDrawingGroup struct.
    /// </summary>
    /// <param name="doc">The pdf document to render.</param>
    /// <param name="oneBasedPageNumber">The page number of the page to render.  First page is page 1.</param>
    public RenderToDrawingGroup(DocumentRenderer doc, int oneBasedPageNumber)
    {
        this.doc = doc;
        this.oneBasedPageNumber = oneBasedPageNumber;
    }

    /// <summary>
    /// Render a PDF page to a PNG image.
    /// </summary>
    /// <param name="stream">Writeable stream to receive the PNG bits.</param>
    public async ValueTask RenderToPngStreamAsync(Stream stream) =>
        await WriteToBufferStream(
                DrawingGroupToBitmap(await RenderAsync())).CreateReader()
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

    /// <summary>
    /// Render the current document and page to a DrawingImage
    /// </summary>
    /// <returns>The rendered page.</returns>
    public async ValueTask<DrawingImage> RenderToDrawingImageAsync()
    {
        var image = new DrawingImage(await RenderAsync());
        image.Freeze();
        return image;
    }

    /// <summary>
    /// Render the current document and page to a DrawingGroup
    /// </summary>
    /// <returns>The rendered page.</returns>
    public async ValueTask<DrawingGroup> RenderAsync()
    {
        var dg = new DrawingGroup();
        await RenderToAsync(dg);
        dg.Freeze();
        return dg;
    }

    private async ValueTask RenderToAsync(DrawingGroup dg)
    {
        using var dc = dg.Open();
        await RenderToAsync(dc, dg);
    }
    

    private  ValueTask RenderToAsync(DrawingContext dc, DependencyObject sizeTarget)
    {
        AwaitConfig.ResumeOnCalledThread(true);
        var d2 = doc;
        return doc.RenderPageToAsync(oneBasedPageNumber, (rect, preTransform) =>
        {
            var innerRenderer = new WpfRenderTarget(dc);
            d2.InitializeRenderTarget(innerRenderer, rect, rect.Width, rect.Height, preTransform);
            PageDisplay.SetPageSize(sizeTarget, new PageSize(rect.Width, rect.Height));
            return innerRenderer;
        });
   }

    /// <summary>
    /// Render the current document and page to a DrawingVisual
    /// </summary>
    /// <returns>The rendered page.</returns>
    public async ValueTask<DrawingVisual> RenderToDrawingVisualAsync()
    {
        var ret = new DrawingVisual();
        using var dc = ret.RenderOpen();
        await RenderToAsync(dc, ret);
        return ret;
    }
}