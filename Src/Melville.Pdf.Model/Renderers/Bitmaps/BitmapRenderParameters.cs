using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal record struct BitmapRenderParameters(PdfValueStream Stream, IHasPageAttributes Page, DeviceColor FillColor,
    int Width, int Height)
{
    public async ValueTask<double[]?> DecodeAsync() =>
        Stream.TryGetValue(KnownNames.DecodeTName, out var arrayTask) &&
        (await arrayTask.CA()).TryGet(out PdfValueArray array) 
            ? await array.CastAsync<double>().CA()
            : null;

    public async ValueTask<bool> IsImageMaskAsync() =>
        await Stream.GetOrDefaultAsync(KnownNames.ImageMaskTName, false).CA();

    public async ValueTask<IColorSpace> ColorSpaceAsync() =>
        Stream.ContainsKey(KnownNames.ColorSpaceTName)?
        await new ColorSpaceFactory(Page)
            .FromNameOrArrayAsync(await Stream[KnownNames.ColorSpaceTName].CA()).CA() :
        DeviceRgb.Instance;

    public ValueTask<long> BitsPerComponentAsync() =>
        Stream.GetOrDefaultAsync(KnownNames.BitsPerComponentTName, 8L);

    public ValueTask<PdfDirectValue> MaskAsync() => 
        Stream.GetOrNullAsync(KnownNames.MaskTName);
    public ValueTask<PdfDirectValue> SoftMaskAsync() => 
        Stream.GetOrNullAsync(KnownNames.SMaskTName);

    public ValueTask<bool> ShouldInterpolateAsync() =>
        Stream.GetOrDefaultAsync(KnownNames.InterpolateTName, false);
}