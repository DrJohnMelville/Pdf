using Melville.Pdf.Model.Documents;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp
{
    public static class RenderWithSkia
    {
        public static async ValueTask ToPngStream(
            PdfPage page, Stream target, int width = -1, int height = 1)
        {
            using var surface = await ToSurface(page, width, height);
            using var image = surface.Snapshot();
            using var data = image.Encode();
            data.SaveTo(target);
        }

        public static ValueTask<SKSurface> ToSurface(PdfPage page, int width = -1, int height = 1)
        {
            var surface = SKSurface.Create(new SKImageInfo(100, 100));
            surface.Canvas.Clear(SKColors.Coral);
            return new ValueTask<SKSurface>(surface);
        }
    }
}