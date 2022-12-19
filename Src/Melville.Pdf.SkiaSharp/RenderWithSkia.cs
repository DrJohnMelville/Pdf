using System.Numerics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp
{
    /// <summary>
    /// This is a facade class that is used to render pdf document pages using Google's skia drawing library.
    /// </summary>
    public static class RenderWithSkia
    {
        /// <summary>
        /// Render a given page of a pdf document to a PNG formatted image
        ///
        /// Height and width are optional parameters.  If only one is specified the other will be computed
        /// to maintain the aspect ratio of the PDF viewport.  If neither is specified, the image will be
        /// the size of the PDF viewport.
        /// </summary>
        /// <param name="doc">The PDF document to render</param>
        /// <param name="oneBasedPageNumber">The page number to render, first page being page 1.</param>
        /// <param name="target">A writable stream to receive the output PNG bits.</param>
        /// <param name="width">Optional parameter for the width of the desired image.</param>
        /// <param name="height">Optional parameter for the height of the desired image.</param>
        public static async ValueTask ToPngStreamAsync(
            DocumentRenderer doc, int oneBasedPageNumber, Stream target, int width = -1, int height = -1)
        {
            using var surface = await ToSurfaceAsync(doc, oneBasedPageNumber, width, height).CA();
            using var image = surface.Snapshot();
            using var data = image.Encode();
            data.SaveTo(target);
        }

        /// <summary>
        /// Render a given page of a pdf document to a Skia SKSurface.
        ///
        /// Height and width are optional parameters.  If only one is specified the other will be computed
        /// to maintain the aspect ratio of the PDF viewport.  If neither is specified, the image will be
        /// the size of the PDF viewport.
        /// </summary>
        /// <param name="doc">The PDF document to render</param>
        /// <param name="oneBasedPageNumber">The page number to render, first page being page 1.</param>
        /// <param name="width">Optional parameter for the width of the desired image.</param>
        /// <param name="height">Optional parameter for the height of the desired image.</param>
        /// <returns>A SKSurface object with the selected page rendered upon it.</returns>
        public static async ValueTask<SKSurface> ToSurfaceAsync(
            DocumentRenderer doc, int oneBasedPageNumber, int width = -1, int height = -1)
        {
            SKSurface surface = null!;
            await doc.RenderPageTo(oneBasedPageNumber, (rect, adjustOutput) =>
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