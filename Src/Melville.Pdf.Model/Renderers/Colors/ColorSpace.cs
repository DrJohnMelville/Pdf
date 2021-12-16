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
    public static ValueTask<IColorSpace> ParseColorSpace(PdfName colorSpaceName, in PdfPage page)
    {
        #warning both cmyk and CalGray need to honor the current rendering intent
        return colorSpaceName.GetHashCode() switch
        {
            KnownNameKeys.DeviceGray => new(DeviceGray.Instance),
            KnownNameKeys.DeviceRGB => new(DeviceRgb.Instance),
            KnownNameKeys.DeviceCMYK => CreateCmykColorSpace(),
            _ => FromArray(page, colorSpaceName)
        };
    }
    private static IColorSpace? cmykColorSpacel;
    public static async ValueTask<IColorSpace> CreateCmykColorSpace() => cmykColorSpacel ??= 
        new IccColorspaceWithBlackDefault((await IccProfileLibrary.ReadCmyk()).TransformTo(
            await IccProfileLibrary.ReadSrgb()));

    private static async ValueTask<IColorSpace> FromArray(PdfPage page, PdfName colorSpaceName)
    {
        var obj = await page.GetResourceObject(ResourceTypeName.ColorSpace, colorSpaceName);
        return obj is PdfArray array? await FromArray(array, page): DeviceGray.Instance;
    }

    private static async ValueTask<IColorSpace> FromArray(PdfArray array, PdfPage page) =>
        (await array.GetAsync<PdfName>(0)).GetHashCode() switch
        {
            KnownNameKeys.CalGray => await CalGray.Parse(await array.GetAsync<PdfDictionary>(1)),
            // for monitors ignore CalRGB see standard section 8.6.5.7
            KnownNameKeys.CalRGB => DeviceRgb.Instance, 
            KnownNameKeys.CalCMYK => await CreateCmykColorSpace(), // standard section 8.6.5.1
            KnownNameKeys.Lab => await LabColorSpace.Parse(await array.GetAsync<PdfDictionary>(1)),
            KnownNameKeys.ICCBased => await IccProfileColorSpace.Parse(await array.GetAsync<PdfStream>(1)),
            KnownNameKeys.Indexed => await IndexedColorSpace.Parse(array, page),
            _=> throw new PdfParseException("Unrecognized Colorspace")
        };

}