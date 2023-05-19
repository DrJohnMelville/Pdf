using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal record struct BitmapRenderParameters(PdfStream Stream, IHasPageAttributes Page, DeviceColor FillColor,
    int Width, int Height)
{
    public async ValueTask<double[]?> DecodeAsync() =>
        Stream.TryGetValue(KnownNames.Decode, out var arrayTask) &&
        await arrayTask.CA() is PdfArray array
            ? await array.AsDoublesAsync().CA()
            : null;

    public async ValueTask<bool> IsImageMaskAsync() =>
        await Stream.GetOrDefaultAsync(KnownNames.ImageMask, PdfBoolean.False).CA()
            == PdfBoolean.True;

    public async ValueTask<IColorSpace> ColorSpaceAsync() =>
        Stream.ContainsKey(KnownNames.ColorSpace)?
        await new ColorSpaceFactory(Page)
            .FromNameOrArray(await Stream[KnownNames.ColorSpace].CA()).CA() :
        DeviceRgb.Instance;

    public ValueTask<long> BitsPerComponentAsync() =>
        Stream.GetOrDefaultAsync(KnownNames.BitsPerComponent, 8);

    public ValueTask<PdfObject> MaskAsync() => Stream.GetOrNullAsync(KnownNames.Mask);
    public ValueTask<PdfObject> SoftMaskAsync() => 
        Stream.GetOrNullAsync(KnownNames.SMask);

    public ValueTask<bool> ShouldInterpolateAsync() =>
        Stream.GetOrDefaultAsync(KnownNames.Interpolate, false);
}