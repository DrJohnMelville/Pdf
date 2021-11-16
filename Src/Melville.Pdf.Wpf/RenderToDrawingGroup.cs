using System.IO.Pipelines;
using System.Windows.Media;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf;

public static class RenderToDrawingGroup
{
    public static async ValueTask<DrawingGroup> Render(PdfPage page)
    {
        var dg = new DrawingGroup();
        using (var dc = dg.Open())
        {
            await Render(page, dc);
        }
        dg.Freeze();
        return dg;
    }

    private static async ValueTask Render(PdfPage page, DrawingContext dc)
    {
        var stateStack = new GraphicsStateStack();
        var renderTarget = new WpfRenderTarget(dc, stateStack, page);
        var engine = new RenderEngine(page, renderTarget, stateStack);
        
        var rect = await page.GetBoxAsync(BoxName.CropBox);
        if (!rect.HasValue) return;
        renderTarget.SetBackgroundRect(rect.Value);

        var csp = new ContentStreamParser(engine);
        await csp.Parse(PipeReader.Create(await page.GetContentBytes()));
    }
}