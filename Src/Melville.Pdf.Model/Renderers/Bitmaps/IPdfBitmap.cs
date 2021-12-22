using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public interface IPdfBitmap
{
    int Width { get; }
    int Height { get; }

    /// <summary>
    /// Fill the bitmap pointed to by buffer.
    /// This is implemented as an unsafe pointer operation so that I can quickly fill native buffers,
    /// deep in the graphics stack.
    /// </summary>
    /// <param name="buffer">A byte pointer which must point to the beginning of a buffer that
    /// is Width * Height *4 bytes long</param>
    unsafe ValueTask RenderPbgra(byte* buffer);
}

    public static class PdfBitmapOperatons
    {
    public static int ReqiredBufferSize(this IPdfBitmap bitmap) => 4 * TotalPixels(bitmap);
    public static int TotalPixels(this IPdfBitmap bitmap) => bitmap.Width * bitmap.Height;

    public static async ValueTask<IPdfBitmap> WrapForRenderingAsync(
        this PdfStream stream, PdfPage page, DeviceColor fillColor)
    {
        
        var reader = new PdfBitmapWrapper(PipeReader.Create(await stream.StreamContentAsync()),
            (int)await stream.GetOrDefaultAsync(KnownNames.Width, 1),
            (int)await stream.GetOrDefaultAsync(KnownNames.Height, 1),
            await GetByteWriterAsync(stream, page, fillColor));
        
        return await stream.GetOrDefaultAsync<PdfObject>(KnownNames.Mask, PdfTokenValues.Null) switch
        {
            PdfStream maskImage => new MaskedBitmapWrapper(reader, await MaskBitmap.Create(maskImage, page)),
            _ => reader
        };
    }

    private static async ValueTask<IByteWriter> GetByteWriterAsync(
        PdfStream stream, PdfPage page, DeviceColor fillColor)
    {
        var decode = stream.TryGetValue(KnownNames.Decode, out var arrayTask) && await arrayTask is PdfArray array
            ? await array.AsDoublesAsync()
            : null;
        if (await stream.GetOrDefaultAsync(KnownNames.ImageMask, PdfBoolean.False) == PdfBoolean.True)
        {
            return new StencilWriter(decode, fillColor);
        }
        var colorSpace = await ColorSpaceFactory.FromNameOrArray(
            await stream[KnownNames.ColorSpace], page);
        var bitsPerComponent = (int)await stream.GetOrDefaultAsync(KnownNames.BitsPerComponent, 8);
        var mask = await stream.GetOrDefaultAsync<PdfObject>(KnownNames.Mask, PdfTokenValues.Null);

        if (CanUseFastWriter(colorSpace, bitsPerComponent, decode, mask))
            return FastBitmapWriterRGB8.Instance;

        var compoentWriter = await WrapWithMask(mask, 
            CreateComponentWriter(colorSpace, decode, bitsPerComponent));

        return CreateByteWriter(bitsPerComponent, compoentWriter);
    }

    private static bool CanUseFastWriter(
        IColorSpace colorSpace, int bitsPerComponent, double[]? decode, PdfObject mask) =>
        colorSpace == DeviceRgb.Instance && 
        bitsPerComponent == 8 && 
        IsDefaultDecode(decode) && 
        mask == PdfTokenValues.Null;

    private static IByteWriter CreateByteWriter(int bitsPerComponent, IComponentWriter writer) =>
        bitsPerComponent == 16 ? new ByteWriter16(writer) : new NBitByteWriter(writer, bitsPerComponent);

    private static IComponentWriter CreateComponentWriter(
        IColorSpace colorSpace, double[]? decode, int bitsPerComponent) =>
        new ComponentWriter(
            new ClosedInterval(0,(1 << bitsPerComponent)- 1), 
            SpecifiedOrDefaultDecodeIntervals(colorSpace, decode, bitsPerComponent), colorSpace);

    private static async ValueTask<IComponentWriter> WrapWithMask(
        PdfObject mask, IComponentWriter componentWriter) =>
        mask switch
        {
            PdfArray maskArr => new ColorMaskComponentWriter(componentWriter,
                await maskArr.AsIntsAsync()),
            _ => componentWriter
        };

    private static ClosedInterval[] SpecifiedOrDefaultDecodeIntervals(
        IColorSpace colorSpace, double[]? decode, int bitsPerComponent) =>
        decode == null ? 
            colorSpace.DefaultOutputIntervals(bitsPerComponent) : 
            ComputeDecodeIntervals(decode);

    private static ClosedInterval[] ComputeDecodeIntervals(double[] decode)
    {
        CheckDecodeArrayLength(decode);
        var ret = new ClosedInterval[decode.Length / 2];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = new ClosedInterval(decode[2 * i], decode[2 * i + 1]);
        }

        return ret;
    }

    private static void CheckDecodeArrayLength(double[] decode)
    {
        if (decode.Length % 2 == 1)
            throw new PdfParseException("Decode array must have an even number of elements");
    }

    private static bool IsDefaultDecode(double[]? decode) =>
        decode == null || IsExplicitDefaultDecode(decode);

    private static bool IsExplicitDefaultDecode(double[] decode) =>
        decode.Length == 6 &&
        decode[0] == 0.0 && decode[1] == 1.0 &&
        decode[2] == 0.0 && decode[3] == 1.0 &&
        decode[4] == 0.0 && decode[5] == 1.0;
    }