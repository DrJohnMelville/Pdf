using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal record struct BitmapRenderParameters(PdfStream Stream, IHasPageAttributes Page, DeviceColor FillColor,
    int Width, int Height)
{
}

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


    public static async ValueTask<IPdfBitmap> WrapForRenderingAsync(
        this PdfStream stream, IHasPageAttributes page, DeviceColor fillColor)
    {
        var streamAttrs = new BitmapRenderParameters(
            stream, page, fillColor,
            (int)await stream.GetOrDefaultAsync(KnownNames.Width, 1).CA(),
            (int)await stream.GetOrDefaultAsync(KnownNames.Height, 1).CA()
        );

        return new PdfBitmapWrapper(PipeReader.Create(await stream.StreamContentAsync().CA()),
            streamAttrs.Width, streamAttrs.Height,
            await stream.GetOrDefaultAsync(KnownNames.Interpolate, false).CA(),
            await GetByteWriterAsync(streamAttrs).CA());
    }
    
    private static async ValueTask<IByteWriter> GetByteWriterAsync(BitmapRenderParameters attr)
    {
        var decode = attr.Stream.TryGetValue(KnownNames.Decode, out var arrayTask) &&
                     await arrayTask.CA() is PdfArray array
            ? await array.AsDoublesAsync().CA()
            : null;

        if (await attr.Stream.GetOrDefaultAsync(KnownNames.ImageMask, PdfBoolean.False).CA() == PdfBoolean.True)
        {
            return new StencilWriter(decode, attr.FillColor);
        }

        var colorSpace = await new ColorSpaceFactory(attr.Page).FromNameOrArray(
            await attr.Stream[KnownNames.ColorSpace].CA()).CA();
        var bitsPerComponent =
            (int)await attr.Stream.GetOrDefaultAsync(KnownNames.BitsPerComponent, 8).CA();
        var mask = await attr.Stream.GetOrNullAsync(KnownNames.Mask).CA();
        var softMask = await attr.Stream.GetOrNullAsync(KnownNames.SMask).CA();
        if (CanUseFastWriter(colorSpace, bitsPerComponent, decode, mask, softMask))
            return FastBitmapWriterRGB8.Instance;

        var componentWriter = await WrapWithMask(mask, attr,
            CreateComponentWriter(colorSpace, decode, bitsPerComponent)).CA();
        componentWriter = await WrapWithSoftMask(softMask, attr, componentWriter).CA();

        return CreateByteWriter(bitsPerComponent, componentWriter);
    }

    private static bool CanUseFastWriter(
        IColorSpace colorSpace, int bitsPerComponent, double[]? decode, PdfObject mask, PdfObject sMask) =>
        colorSpace == DeviceRgb.Instance &&
        bitsPerComponent == 8 &&
        DecodeArrayParser.IsDefaultDecode(decode) &&
        mask == PdfTokenValues.Null &&
        sMask == PdfTokenValues.Null;

    private static IByteWriter CreateByteWriter(int bitsPerComponent, IComponentWriter writer) =>
        bitsPerComponent == 16 ? new ByteWriter16(writer) : new NBitByteWriter(writer, bitsPerComponent);

    private static IComponentWriter CreateComponentWriter(
        IColorSpace colorSpace, double[]? decode, int bitsPerComponent) =>
        new ComponentWriter(
            new ClosedInterval(0, (1 << bitsPerComponent) - 1),
            DecodeArrayParser.SpecifiedOrDefaultDecodeIntervals(
                colorSpace, decode, bitsPerComponent), colorSpace);

    private static async ValueTask<IComponentWriter> WrapWithMask(
        PdfObject mask, BitmapRenderParameters attr, IComponentWriter componentWriter) =>
        mask switch
        {
            PdfArray maskArr => new ColorMaskComponentWriter(componentWriter,
                await maskArr.AsIntsAsync().CA()),
            PdfStream str => new HardMaskedBitmapWriter(componentWriter,
                await MaskBitmap.Create(str, attr.Page).CA(), attr.Width, attr.Height),
            _ => componentWriter
        };
    private static async ValueTask<IComponentWriter> WrapWithSoftMask(
        PdfObject mask, BitmapRenderParameters attr, IComponentWriter componentWriter) =>
        mask switch
        {
            PdfStream str => new SoftMaskedBitmapWriter(componentWriter,
                await MaskBitmap.Create(str, attr.Page).CA(), attr.Width, attr.Height),
            _ => componentWriter
        };
}