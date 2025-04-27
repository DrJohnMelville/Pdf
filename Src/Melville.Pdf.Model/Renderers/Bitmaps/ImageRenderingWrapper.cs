using System.Collections.Generic;
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
    [FromConstructor] private readonly IReadOnlyList<double>? decode;
    [FromConstructor] private readonly bool isImageMask;
    [FromConstructor] private readonly IColorSpace colorSpace;
    [FromConstructor] private readonly int bitsPerComponent;
    [FromConstructor] private readonly PdfDirectObject mask;
    [FromConstructor] private readonly PdfDirectObject softMask;
    [FromConstructor] private readonly bool shouldInterpolate;
    [FromConstructor] private readonly BitmapRenderParameters attr;

    public async ValueTask<IPdfBitmap> AsPdfBitmapAsync() =>
        await WrapWithSoftMaskAsync(
        await WrapWithHardMaskAsync(
        new PdfBitmapWrapper(attr, shouldInterpolate, Wrap, colorSpace)).CA()).CA();

    private IByteWriter Wrap(IColorSpace finalColorspace)
    {
        if (isImageMask) return new StencilWriter(decode, attr.FillColor);

        if (CanUseFastWriter(finalColorspace)) return FastBitmapWriterRGB8.Instance;

        return CreateByteWriter(CreateComponentWriter(finalColorspace));
    }

    private IByteWriter CreateByteWriter(ComponentWriter writer) =>
        bitsPerComponent switch
        {
            16 => new ByteWriter16(writer),
            8 => new ByteWriter8(writer),
            _ => new NBitByteWriter(writer, bitsPerComponent)
        };

    private bool CanUseFastWriter(IColorSpace finalColorspace) =>
        finalColorspace == DeviceRgb.Instance &&
        bitsPerComponent == 8 &&
        DecodeArrayParser.IsDefaultDecode(decode);

    private ComponentWriter CreateComponentWriter(IColorSpace finalColorspace) =>
        new(new ClosedInterval(0, (1 << bitsPerComponent) - 1),
            DecodeArrayParser.SpecifiedOrDefaultDecodeIntervals(
                colorSpace, decode, bitsPerComponent), finalColorspace);

    private async ValueTask<IPdfBitmap> WrapWithHardMaskAsync(
        IPdfBitmap writer) => mask switch
        {
            var x when x.TryGet(out PdfArray? maskArr) => 
                 new SelfMaskAdjuster<ColorRangeMaskType>(writer,
                     new ColorRangeMaskType(
                     await maskArr.CastAsync<int>().CA(), bitsPerComponent, colorSpace)),                
            var x when x.TryGet(out PdfStream? str) => await CreateMaskWriterAsync<HardMask>(writer, str).CA(),
            _ => writer
        };

    private ValueTask<IPdfBitmap> WrapWithSoftMaskAsync(
        IPdfBitmap writer) => softMask.TryGet(out PdfStream? str) ?
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