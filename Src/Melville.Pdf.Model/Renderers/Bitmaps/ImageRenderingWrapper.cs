using System.IO.Pipelines;
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
    [FromConstructor] private readonly bool shouldInterpolate;
    [FromConstructor] private readonly BitmapRenderParameters attr;

    public async ValueTask<IPdfBitmap> AsPdfBitmapAsync() =>
        await WrapWithSoftMaskAsync(
        await WrapWithHardMaskAsync(
        new PdfBitmapWrapper(
            PipeReader.Create(await attr.Stream.StreamContentAsync().CA()),
            attr.Width, attr.Height, shouldInterpolate, Wrap())).CA()).CA();

    private IByteWriter Wrap()
    {
        if (isImageMask) return new StencilWriter(decode, attr.FillColor);

        if (CanUseFastWriter()) return FastBitmapWriterRGB8.Instance;

        return CreateByteWriter(CreateComponentWriter());
    }

    private IByteWriter CreateByteWriter(ComponentWriter writer) =>
        bitsPerComponent == 16 ?
            new ByteWriter16(writer) :
            new NBitByteWriter(writer, bitsPerComponent);


    private bool CanUseFastWriter() =>
        colorSpace == DeviceRgb.Instance &&
        bitsPerComponent == 8 &&
        DecodeArrayParser.IsDefaultDecode(decode);

    private ComponentWriter CreateComponentWriter() =>
        new(new ClosedInterval(0, (1 << bitsPerComponent) - 1),
            DecodeArrayParser.SpecifiedOrDefaultDecodeIntervals(
                colorSpace, decode, bitsPerComponent), colorSpace);

    private async ValueTask<IPdfBitmap> WrapWithHardMaskAsync(
        IPdfBitmap writer) => mask switch
        {
            PdfArray maskArr => 
                 new SelfMaskAdjuster<ColorRangeMaskType>(writer,
                     new ColorRangeMaskType(
                     await maskArr.AsIntsAsync().CA(), bitsPerComponent, colorSpace)),                
            PdfStream str => await CreateMaskWriterAsync<HardMask>(writer, str).CA(),
            _ => writer
        };

    private ValueTask<IPdfBitmap> WrapWithSoftMaskAsync(
        IPdfBitmap writer) => softMask is PdfStream str ?
            CreateMaskWriterAsync<SoftMask>(writer, str) :
            ValueTask.FromResult(writer);

    private async ValueTask<IPdfBitmap> CreateMaskWriterAsync<T>(
        IPdfBitmap target, PdfStream str)
        where T : IMaskType, new()
    {
        var maskBitmap = await MaskBitmap.CreateAsync(str, attr.Page).CA();
        return
            maskBitmap.IsSameSizeAs(target)
                ? new SameSizeMaskAdjuster<T>(target, maskBitmap, new T())
                : new ArbitrarySizeMaskAdjuster<T>(target, maskBitmap, new T());
    }
}