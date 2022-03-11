using System;
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
    public static ValueTask<IColorSpace> ParseColorSpace(PdfName colorSpaceName, IHasPageAttributes page)
    {
        var code = colorSpaceName.GetHashCode();
        return code switch
        {
            KnownNameKeys.DeviceGray  or KnownNameKeys.DeviceRGB or KnownNameKeys.DeviceCMYK => 
                SpacesWithoutParameters(code, page),
            _ => FromArray(page, colorSpaceName)
        };
    }

    public static async ValueTask<IColorSpace> SearchForDefault(PdfName name, IHasPageAttributes page, 
        Func<ValueTask<IColorSpace>> space) =>
        await (await page.GetResourceAsync(ResourceTypeName.ColorSpace, name).CA() is PdfArray array
            ?  FromArray(array, page): space()).CA();

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
        await FromMemory((await array.AsAsync<PdfObject>().CA()).AsMemory(), page).CA();

    private static ValueTask<IColorSpace> FromMemory(Memory<PdfObject> memory, IHasPageAttributes page)
    {
        var array = memory.Span;
        if (array.Length == 0) return new(DeviceRgb.Instance);
        if (array[0] is not PdfName name) throw new PdfParseException("Name expected in colorspace array");
        return name.GetHashCode() switch
        {
            KnownNameKeys.CalGray => CalGray.Parse(ColorSpaceParameterAs<PdfDictionary>(array)),
            KnownNameKeys.Lab => LabColorSpace.ParseAsync(ColorSpaceParameterAs<PdfDictionary>(array)),
            KnownNameKeys.ICCBased => IccProfileColorSpace.ParseAsync(ColorSpaceParameterAs<PdfStream>(array)),
            KnownNameKeys.Indexed => IndexedColorSpace.ParseAsync(memory, page),
            KnownNameKeys.Separation => SeparationParser.ParseSeparationAsync(memory, page),
            KnownNameKeys.DeviceN => SeparationParser.ParseDeviceNAsync(memory, page),
            var other => SpacesWithoutParameters(other, page)
        };
    }

    private static ValueTask<IColorSpace> SpacesWithoutParameters(int nameHashCode, IHasPageAttributes page) => nameHashCode switch
    {
        // for monitors ignore CalRGB see standard section 8.6.5.7
        KnownNameKeys.CalRGB => new(DeviceRgb.Instance),
        KnownNameKeys.CalCMYK => CreateCmykColorSpaceAsync(), // standard section 8.6.5.1
        KnownNameKeys.DeviceGray => SearchForDefault(KnownNames.DefaultGray, page, static ()=>new(DeviceGray.Instance)),
        KnownNameKeys.DeviceRGB => SearchForDefault(KnownNames.DefaultRGB, page, static ()=>new(DeviceRgb.Instance)),
        KnownNameKeys.DeviceCMYK =>  SearchForDefault(KnownNames.DefaultCMYK, page, CreateCmykColorSpaceAsync),
        _ => throw new PdfParseException("Unrecognized Colorspace")
    };
        

    private static T ColorSpaceParameterAs<T>(in ReadOnlySpan<PdfObject> array) where T:PdfObject =>
        array[1] as T ?? throw new PdfParseException("Dictionary Expected");
}