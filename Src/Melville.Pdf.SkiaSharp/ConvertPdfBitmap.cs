using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.Model.Renderers.Bitmaps;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp
{
    /// <summary>
    /// This extension class converts IPdfBitmap into a Skia SkBitmap type.
    /// </summary>
    public static class ConvertPdfBitmap
    {
        /// <summary>
        /// Convert an IPdfBitmap to a Skia SkBitmap
        /// </summary>
        /// <param name="bitmap">An IPdfBitmap to be rendered into a Skia bitmap</param>
        /// <returns>The resulting SkBitmap -- which the caller must dispose.</returns>
        public static async ValueTask<SKBitmap> ToSkBitmapAsync(this IPdfBitmap bitmap)
        {
            var skBitmap = ScreenFormatBitmap(bitmap);
            await FillBitmapAsync(bitmap, skBitmap).CA();
            return skBitmap;
        }

        private static SKBitmap ScreenFormatBitmap(IPdfBitmap bitmap) =>
            new(new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Bgra8888,
                SKAlphaType.Premul, SKColorSpace.CreateSrgb()));

        private static unsafe ValueTask FillBitmapAsync(IPdfBitmap bitmap, SKBitmap skBitmap) =>
            bitmap.RenderPbgra((byte*)skBitmap.GetPixels().ToPointer());


    }
}