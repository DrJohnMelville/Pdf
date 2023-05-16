using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

/// <summary>
/// Create a PdfBitmap from a bitmap PdfStream and a few related operations
/// </summary>
public static class PdfBitmapOperations
{
    /// <summary>
    /// Buffer size needed to render the bitmap.
    /// </summary>
    /// <param name="bitmap">Bitmap to inquire about</param>
    /// <returns>Buffer size, in bytes, needed to render the bitmap.</returns>
    public static int ReqiredBufferSize(this IPdfBitmap bitmap) => 4 * TotalPixels(bitmap);
    /// <summary>
    /// Number of pxels in the bitmap/
    /// </summary>
    /// <param name="bitmap">Bitmap to inquire about</param>
    public static int TotalPixels(this IPdfBitmap bitmap) => bitmap.Width * bitmap.Height;

    /// <summary>
    /// Create a IPdfBitmap from a stream
    /// </summary>
    /// <param name="stream">The stream containing the bitmap.</param>
    /// <param name="fillColor">The background color for the bitmap.</param>
    /// <returns></returns>
    public static ValueTask<IPdfBitmap> WrapForRenderingAsync(
        this PdfStream stream, DeviceColor fillColor) =>
        WrapForRenderingAsync(stream, NoPageContext.Instance, fillColor);

    internal static async ValueTask<IPdfBitmap> WrapForRenderingAsync(
        this PdfStream stream, IHasPageAttributes page, DeviceColor fillColor)
    {
        var streamAttrs = new BitmapRenderParameters(
            stream, page, fillColor,
            (int)await stream.GetOrDefaultAsync(KnownNames.Width, 1).CA(),
            (int)await stream.GetOrDefaultAsync(KnownNames.Height, 1).CA()
        );

        return new PdfBitmapWrapper(
            PipeReader.Create(await stream.StreamContentAsync().CA()),
            streamAttrs.Width, streamAttrs.Height,
            await stream.GetOrDefaultAsync(KnownNames.Interpolate, false).CA(),
            await GetByteWriterAsync(streamAttrs).CA());
    }
    private static async ValueTask<IByteWriter> GetByteWriterAsync(BitmapRenderParameters attr) =>
        await new ImageRenderingWrapper(
            await attr.DecodeAsync().CA(),
            await attr.IsImageMaskAsync().CA(),
            await attr.ColorSpaceAsync().CA(),
            (int)await attr.BitsPerComponentAsync().CA(),
            await attr.MaskAsync().CA(),
            await attr.SoftMaskAsync().CA(),
            attr
            ).Wrap().CA();
}