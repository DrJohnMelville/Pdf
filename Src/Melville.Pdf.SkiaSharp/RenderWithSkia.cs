using System.Numerics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.Model.Renderers;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp
{
    public static class RenderWithSkia
    {
        public static async ValueTask ToPngStreamAsync(
            DocumentRenderer doc, int page, Stream target, int width = -1, int height = -1)
        {
            using var surface = await ToSurfaceAsync(doc, page, width, height).CA();
            using var image = surface.Snapshot();
            using var data = image.Encode();
            data.SaveTo(target);
        }

        public static async ValueTask<SKSurface> ToSurfaceAsync(DocumentRenderer doc, int page, int width = -1, int height = -1)
        {
            SKSurface surface = null!;
            await doc.RenderPageTo(page, (rect, adjustOutput) =>
            {
                (width, height) = doc.ScalePageToRequestedSize(rect, new Vector2(width, height));
                surface = SKSurface.Create(new SKImageInfo(width, height));

                var target = new SkiaRenderTarget(surface.Canvas);
                doc.InitializeRenderTarget(target, rect, width, height, adjustOutput);
                return target;
            }).CA();
            return surface;
        }
    }
}