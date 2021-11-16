using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp
{
    public static class RenderWithSkia
    {
        public static async ValueTask ToPngStream(
            PdfPage page, Stream target, int width = -1, int height = -1)
        {
            using var surface = await ToSurface(page, width, height);
            using var image = surface.Snapshot();
            using var data = image.Encode();
            data.SaveTo(target);
        }

        public static async ValueTask<SKSurface> ToSurface(PdfPage page, int width = -1, int height = -1)
        {
            var rect = await page.GetBoxAsync(BoxName.CropBox);
            if (!rect.HasValue) return SKSurface.Create(new SKImageInfo(1, 1));
            (width, height) = AdjustSize(rect.Value, width, height);
            var surface = SKSurface.Create(new SKImageInfo(width, height));
            surface.Canvas.Clear(SKColors.Coral);
            return (surface);
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