using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.FontMappings;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Wpf.FakeUris;

namespace Melville.Pdf.Wpf;

public class RenderToDrawingGroup:IDisposable
{ 
    private readonly TempFontDirectory fontCache = new();
    private readonly IDefaultFontMapper fontMapper;

    public RenderToDrawingGroup(IDefaultFontMapper? fontMapper = null)
    {
        this.fontMapper = fontMapper ?? new WindowsDefaultFonts();
    }

    public void Dispose()
    {
        fontCache.Dispose();
    }

    public async ValueTask RenderToPngStream(PdfPage page, Stream stream) =>
        await WriteToBufferStream(
                DrawingGroupToBitmap(await Render(page))).CreateReader()
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

    public async ValueTask<DrawingGroup> Render(PdfPage page)
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

    private async Task RenderTo(
        PdfPage page, DrawingGroup dg)
    {
        using var dc = dg.Open();
        await RenderTo(page, dc);
    }

    private async ValueTask RenderTo(PdfPage page, DrawingContext dc, 
        IDefaultFontMapper? defaultFontMapper = null)
    { 
        var rect = await page.GetBoxAsync(BoxName.CropBox);
        if (!rect.HasValue) return;
       
        
        var renderTarget = new WpfRenderTarget(dc, new GraphicsStateStack(), page, 
            fontCache);
        renderTarget.SetBackgroundRect(rect.Value);

        await page.RenderTo(renderTarget, new FontReader(defaultFontMapper??new WindowsDefaultFonts()));
    }
}