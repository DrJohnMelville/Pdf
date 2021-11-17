using System.Windows.Media;
using Melville.Pdf.LowLevel.Model.Conventions;
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
        var rect = await page.GetBoxAsync(BoxName.CropBox);
        if (!rect.HasValue) return;
       
        var renderTarget = new WpfRenderTarget(dc, new GraphicsStateStack(), page);
        renderTarget.SetBackgroundRect(rect.Value);

        await page.RenderTo(renderTarget);
    }
}