using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Renderers;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp
{
    public static class RenderWithSkia
    {
        public static async ValueTask ToPngStream(
            DocumentRenderer doc, int page, Stream target, int width = -1, int height = -1)
        {
            using var surface = await ToSurface(doc, page, width, height).CA();
            using var image = surface.Snapshot();
            using var data = image.Encode();
            data.SaveTo(target);
        }

        public static async ValueTask<SKSurface> ToSurface(DocumentRenderer doc, int page, int width = -1, int height = -1)
        {
            SKSurface surface = null!;
            await doc.RenderPageTo(page, (rect, adjustOutput) =>
            {
                (width, height) = AdjustSize(rect, width, height);
                surface = SKSurface.Create(new SKImageInfo(width, height));

                var target = new SkiaRenderTarget(surface.Canvas);
                target.SetBackgroundRect(rect, width, height, adjustOutput);
                return target;
            }).CA();
            return surface;
        }

        private static (int width, int height) AdjustSize(in PdfRect rect, int width, int height) =>
            (width, height) switch
            {
                (< 0, < 0) => ((int)rect.Width, (int)rect.Height),
                (< 0, _) => (Scale(rect.Width, height, rect.Height), height),
                (_, < 0) => (width, Scale(rect.Height, width, rect.Width)),
                _ => (width, height)
            };

        private static int Scale(double freeDimension, int setValue, double setDimension) => 
            (int)(freeDimension * (setValue / setDimension));
    }
}