using System.Collections.Generic;
using System.Threading.Tasks;
using CoreJ2K.Color;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.Colors.Profiles;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal record struct BitmapRenderParameters(PdfStream Stream, IHasPageAttributes Page, DeviceColor FillColor,
    int Width, int Height)
{
    public async Task<IReadOnlyList<double>?> DecodeAsync() =>
        Stream.TryGetValue(KnownNames.Decode, out var arrayTask) &&
        (await arrayTask.CA()).TryGet(out PdfArray? array) 
            ? await array.CastAsync<double>().CA()
            : null;

    public async ValueTask<bool> IsImageMaskAsync() =>
        await Stream.GetOrDefaultAsync(KnownNames.ImageMask, false).CA();

    public async ValueTask<IColorSpace> ColorSpaceAsync() =>
        Stream.ContainsKey(KnownNames.ColorSpace)?
        await new ColorSpaceFactory(Page)
            .FromNameOrArrayAsync(await Stream[KnownNames.ColorSpace].CA()).CA() :
        DeviceRgb.Instance;

    public ValueTask<long> BitsPerComponentAsync() =>
        Stream.GetOrDefaultAsync(KnownNames.BitsPerComponent, 8L);

    public ValueTask<PdfDirectObject> MaskAsync() => 
        Stream.GetOrNullAsync(KnownNames.Mask);
    public ValueTask<PdfDirectObject> SoftMaskAsync() => 
        Stream.GetOrNullAsync(KnownNames.SMask);

    public ValueTask<bool> ShouldInterpolateAsync() =>
        Stream.GetOrDefaultAsync(KnownNames.Interpolate, false);
}