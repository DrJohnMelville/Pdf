using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Parsing.AwaitConfiguration;
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
            KnownNameKeys.DeviceGray => await SearchForDefault(KnownNames.DefaultGray, page, DeviceGray.Instance).CA(),
            KnownNameKeys.DeviceRGB => await SearchForDefault(KnownNames.DefaultRGB, page, DeviceRgb.Instance).CA(),
            KnownNameKeys.DeviceCMYK => 
                await SearchForDefault(KnownNames.DefaultCMYK, page, await CreateCmykColorSpaceAsync().CA()).CA(),
            _ => await FromArray(page, colorSpaceName).CA()
        };
    }

    public static async ValueTask<IColorSpace> SearchForDefault(PdfName name, IHasPageAttributes page, IColorSpace space) =>
        await page.GetResourceAsync(ResourceTypeName.ColorSpace, name).CA() is PdfArray array
            ? await FromArray(array, page).CA(): space;

    public static ValueTask<IColorSpace> FromNameOrArray(PdfObject datum, in IHasPageAttributes page) => datum switch
    {
        PdfName name => ParseColorSpace(name, page),
        PdfArray array => FromArray(array, page),
        _ => throw new PdfParseException("Invalid Color space definition")
    };

    private static IColorSpace? cmykColorSpacel;
    public static async ValueTask<IColorSpace> CreateCmykColorSpaceAsync() => cmykColorSpacel ??= 
        new IccColorspaceWithBlackDefault((await IccProfileLibrary.ReadCmyk().CA()).TransformTo(
            await IccProfileLibrary.ReadSrgb().CA()));

    private static async ValueTask<IColorSpace> FromArray(IHasPageAttributes page, PdfName colorSpaceName)
    {
        var obj = await page.GetResourceAsync(ResourceTypeName.ColorSpace, colorSpaceName).CA();
        return obj is PdfArray array? await FromArray(array, page).CA(): DeviceGray.Instance;
    }

    private static async ValueTask<IColorSpace> FromArray(PdfArray array, IHasPageAttributes page) =>
        (await array.GetAsync<PdfName>(0).CA()).GetHashCode() switch
        {
            KnownNameKeys.CalGray => await CalGray.Parse(await array.GetAsync<PdfDictionary>(1).CA()).CA(),
            // for monitors ignore CalRGB see standard section 8.6.5.7
            KnownNameKeys.CalRGB => DeviceRgb.Instance, 
            KnownNameKeys.CalCMYK => await CreateCmykColorSpaceAsync().CA(), // standard section 8.6.5.1
            KnownNameKeys.Lab => await LabColorSpace.ParseAsync(await array.GetAsync<PdfDictionary>(1).CA()).CA(),
            KnownNameKeys.ICCBased => await IccProfileColorSpace.ParseAsync(await array.GetAsync<PdfStream>(1).CA()).CA(),
            KnownNameKeys.Indexed => await IndexedColorSpace.ParseAsync(array, page).CA(),
            KnownNameKeys.Separation => await SeparationParser.ParseSeparationAsync(array, page).CA(),
            KnownNameKeys.DeviceN => await SeparationParser.ParseDeviceNAsync(array, page).CA(),
            _=> throw new PdfParseException("Unrecognized Colorspace")
        };
}