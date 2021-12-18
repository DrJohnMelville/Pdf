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
    public static async ValueTask<IColorSpace> ParseColorSpace(PdfName colorSpaceName, PdfPage page)
    {
        #warning both cmyk and CalGray need to honor the current rendering intent
        return colorSpaceName.GetHashCode() switch
        {
            KnownNameKeys.DeviceGray => await SearchForDefault(KnownNames.DefaultGray, page, DeviceGray.Instance),
            KnownNameKeys.DeviceRGB => await SearchForDefault(KnownNames.DefaultRGB, page, DeviceRgb.Instance),
            KnownNameKeys.DeviceCMYK => 
                await SearchForDefault(KnownNames.DefaultCMYK, page, await CreateCmykColorSpaceAsync()),
            _ => await FromArray(page, colorSpaceName)
        };
    }

    public static async ValueTask<IColorSpace> SearchForDefault(PdfName name, PdfPage page, IColorSpace space) =>
        await page.GetResourceAsync(ResourceTypeName.ColorSpace, name) is PdfArray array
            ? await FromArray(array, page): space;

    public static ValueTask<IColorSpace> FromNameOrArray(PdfObject datum, in PdfPage page) => datum switch
    {
        PdfName name => ParseColorSpace(name, page),
        PdfArray array => FromArray(array, page),
        _ => throw new PdfParseException("Invalid Color space definition")
    };

    private static IColorSpace? cmykColorSpacel;
    public static async ValueTask<IColorSpace> CreateCmykColorSpaceAsync() => cmykColorSpacel ??= 
        new IccColorspaceWithBlackDefault((await IccProfileLibrary.ReadCmyk()).TransformTo(
            await IccProfileLibrary.ReadSrgb()));

    private static async ValueTask<IColorSpace> FromArray(PdfPage page, PdfName colorSpaceName)
    {
        var obj = await page.GetResourceAsync(ResourceTypeName.ColorSpace, colorSpaceName);
        return obj is PdfArray array? await FromArray(array, page): DeviceGray.Instance;
    }

    private static async ValueTask<IColorSpace> FromArray(PdfArray array, PdfPage page) =>
        (await array.GetAsync<PdfName>(0)).GetHashCode() switch
        {
            KnownNameKeys.CalGray => await CalGray.Parse(await array.GetAsync<PdfDictionary>(1)),
            // for monitors ignore CalRGB see standard section 8.6.5.7
            KnownNameKeys.CalRGB => DeviceRgb.Instance, 
            KnownNameKeys.CalCMYK => await CreateCmykColorSpaceAsync(), // standard section 8.6.5.1
            KnownNameKeys.Lab => await LabColorSpace.ParseAsync(await array.GetAsync<PdfDictionary>(1)),
            KnownNameKeys.ICCBased => await IccProfileColorSpace.ParseAsync(await array.GetAsync<PdfStream>(1)),
            KnownNameKeys.Indexed => await IndexedColorSpace.ParseAsync(array, page),
            KnownNameKeys.Separation => await SeparationParser.ParseSeparationAsync(array, page),
            KnownNameKeys.DeviceN => await SeparationParser.ParseDeviceNAsync(array, page),
            _=> throw new PdfParseException("Unrecognized Colorspace")
        };
}