using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal readonly partial struct ImageRenderingWrapper
{
    [FromConstructor] private readonly double[]? decode;
    [FromConstructor] private readonly bool isImageMask;
    [FromConstructor] private readonly IColorSpace colorSpace;
    [FromConstructor] private readonly int bitsPerComponent;
    [FromConstructor] private readonly PdfObject mask;
    [FromConstructor] private readonly PdfObject softMask;
    [FromConstructor] private readonly BitmapRenderParameters attr;
 
    public async ValueTask<IByteWriter> Wrap()
    {
        if (isImageMask) return new StencilWriter(decode, attr.FillColor);

        if (CanUseFastWriter()) return FastBitmapWriterRGB8.Instance;

        return CreateByteWriter(
            await WrapWithSoftMaskAsync(
            await WrapWithHardMaskAsync(CreateComponentWriter()).CA()).CA());
    }

    private IByteWriter CreateByteWriter(IComponentWriter writer) =>
        bitsPerComponent == 16 ?
            new ByteWriter16(writer) :
            new NBitByteWriter(writer, bitsPerComponent);


    private bool CanUseFastWriter() =>
        colorSpace == DeviceRgb.Instance &&
        bitsPerComponent == 8 &&
        DecodeArrayParser.IsDefaultDecode(decode) &&
        mask == PdfTokenValues.Null &&
        softMask == PdfTokenValues.Null;

    private IComponentWriter CreateComponentWriter() =>
        new ComponentWriter(
            new ClosedInterval(0, (1 << bitsPerComponent) - 1),
            DecodeArrayParser.SpecifiedOrDefaultDecodeIntervals(
                colorSpace, decode, bitsPerComponent), colorSpace);

    private async ValueTask<IComponentWriter> WrapWithHardMaskAsync(
        IComponentWriter writer) => mask switch
        {
            PdfArray maskArr => new ColorMaskComponentWriter(writer,
                await maskArr.AsIntsAsync().CA()),
            PdfStream str => await CreateMaskWriter<HardMask>(writer, str).CA(),
            _ => writer
        };

    private ValueTask<IComponentWriter> WrapWithSoftMaskAsync(
        IComponentWriter writer) => softMask is PdfStream str ?
            CreateMaskWriter<SoftMask>(writer, str) :
            ValueTask.FromResult(writer);

    private async ValueTask<IComponentWriter> CreateMaskWriter<T>(
        IComponentWriter componentWriter, PdfStream str)
        where T : IMaskType, new()
    {
        var maskBitmap = await MaskBitmap.Create(str, attr.Page).CA();
        return new MaskedBitmapWriter<T>(componentWriter,
            maskBitmap, attr.Width, attr.Height);
    }
}