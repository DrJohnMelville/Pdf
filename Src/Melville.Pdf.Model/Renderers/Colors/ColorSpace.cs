using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors.Profiles;

namespace Melville.Pdf.Model.Renderers.Colors;

public static class ColorSpaceFactory
{
    public static async ValueTask<IColorSpace> ParseColorSpace(PdfName colorSpaceName, IHasPageAttributes page)
    {
        return colorSpaceName.GetHashCode() switch
        {
            KnownNameKeys.DeviceGray => await SearchForDefault(KnownNames.DefaultGray, page, DeviceGray.Instance).ConfigureAwait(false),
            KnownNameKeys.DeviceRGB => await SearchForDefault(KnownNames.DefaultRGB, page, DeviceRgb.Instance).ConfigureAwait(false),
            KnownNameKeys.DeviceCMYK => 
                await SearchForDefault(KnownNames.DefaultCMYK, page, await CreateCmykColorSpaceAsync().ConfigureAwait(false)).ConfigureAwait(false),
            _ => await FromArray(page, colorSpaceName).ConfigureAwait(false)
        };
    }

    public static async ValueTask<IColorSpace> SearchForDefault(PdfName name, IHasPageAttributes page, IColorSpace space) =>
        await page.GetResourceAsync(ResourceTypeName.ColorSpace, name).ConfigureAwait(false) is PdfArray array
            ? await FromArray(array, page).ConfigureAwait(false): space;

    public static ValueTask<IColorSpace> FromNameOrArray(PdfObject datum, in IHasPageAttributes page) => datum switch
    {
        PdfName name => ParseColorSpace(name, page),
        PdfArray array => FromArray(array, page),
        _ => throw new PdfParseException("Invalid Color space definition")
    };

    private static IColorSpace? cmykColorSpacel;
    public static async ValueTask<IColorSpace> CreateCmykColorSpaceAsync() => cmykColorSpacel ??= 
        new IccColorspaceWithBlackDefault((await IccProfileLibrary.ReadCmyk().ConfigureAwait(false)).TransformTo(
            await IccProfileLibrary.ReadSrgb().ConfigureAwait(false)));

    private static async ValueTask<IColorSpace> FromArray(IHasPageAttributes page, PdfName colorSpaceName)
    {
        var obj = await page.GetResourceAsync(ResourceTypeName.ColorSpace, colorSpaceName).ConfigureAwait(false);
        return obj is PdfArray array? await FromArray(array, page).ConfigureAwait(false): DeviceGray.Instance;
    }

    private static async ValueTask<IColorSpace> FromArray(PdfArray array, IHasPageAttributes page) =>
        (await array.GetAsync<PdfName>(0).ConfigureAwait(false)).GetHashCode() switch
        {
            KnownNameKeys.CalGray => await CalGray.Parse(await array.GetAsync<PdfDictionary>(1).ConfigureAwait(false)).ConfigureAwait(false),
            // for monitors ignore CalRGB see standard section 8.6.5.7
            KnownNameKeys.CalRGB => DeviceRgb.Instance, 
            KnownNameKeys.CalCMYK => await CreateCmykColorSpaceAsync().ConfigureAwait(false), // standard section 8.6.5.1
            KnownNameKeys.Lab => await LabColorSpace.ParseAsync(await array.GetAsync<PdfDictionary>(1).ConfigureAwait(false)).ConfigureAwait(false),
            KnownNameKeys.ICCBased => await IccProfileColorSpace.ParseAsync(await array.GetAsync<PdfStream>(1).ConfigureAwait(false)).ConfigureAwait(false),
            KnownNameKeys.Indexed => await IndexedColorSpace.ParseAsync(array, page).ConfigureAwait(false),
            KnownNameKeys.Separation => await SeparationParser.ParseSeparationAsync(array, page).ConfigureAwait(false),
            KnownNameKeys.DeviceN => await SeparationParser.ParseDeviceNAsync(array, page).ConfigureAwait(false),
            _=> throw new PdfParseException("Unrecognized Colorspace")
        };
}