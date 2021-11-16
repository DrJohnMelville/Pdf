using System.Windows;
using System.Windows.Media;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Wpf;

public class RenderToDrawingGroup
{
    public async ValueTask<DrawingGroup> Render(PdfPage page)
    {
        var dg = new DrawingGroup();
        using (var dc = dg.Open())
        {
            await Render(page, dc);
        }
        dg.Freeze();
        return dg;
    }

    private async ValueTask Render(PdfPage page, DrawingContext dc)
    {
        var rect = await page.GetBoxAsync(BoxName.CropBox);
        if (!rect.HasValue) return;
        SetupBackgroundRect(dc, rect.Value);
    }

    private void SetupBackgroundRect(DrawingContext dc, in PdfRect rect)
    {
        var clipRectangle = new Rect(0,0, rect.Width, rect.Height);
        dc.DrawRectangle(Brushes.White, null, clipRectangle);
        dc.PushClip(new RectangleGeometry(clipRectangle));
        
    }
}
