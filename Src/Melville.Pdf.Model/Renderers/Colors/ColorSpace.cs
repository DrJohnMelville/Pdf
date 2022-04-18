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

public readonly struct ColorSpaceFactory
{
    private readonly IHasPageAttributes page;
    public ColorSpaceFactory(IHasPageAttributes page)
    {
        this.page = page;
    }

    public ValueTask<IColorSpace> ParseColorSpace(PdfName colorSpaceName)
    {
        var code = colorSpaceName.GetHashCode();
        return code switch
        {
            KnownNameKeys.DeviceGray  or KnownNameKeys.DeviceRGB or KnownNameKeys.DeviceCMYK => 
                SpacesWithoutParameters(code),
            _ => LookupInResourceDictionary(colorSpaceName)
        };
    }

    private async ValueTask<IColorSpace> SearchForDefault(PdfName name, Func<ValueTask<IColorSpace>> space) =>
        await (await page.GetResourceAsync(ResourceTypeName.ColorSpace, name).CA() is PdfArray array
            ?  FromArray(array): space()).CA();

    public ValueTask<IColorSpace> FromNameOrArray(PdfObject datum) => datum switch
    {
        PdfName name => ParseColorSpace(name),
        PdfArray array => FromArray(array),
        _ => throw new PdfParseException("Invalid Color space definition")
    };

    private static IColorSpace? cmykColorSpacel;

    public static async ValueTask<IColorSpace> CreateCmykColorSpaceAsync() => cmykColorSpacel ??= 
        new IccColorspaceWithBlackDefault(await (await IccProfileLibrary.ReadCmyk().CA()).DeviceToSrgb().CA());

    private async ValueTask<IColorSpace> LookupInResourceDictionary(PdfName colorSpaceName)
    {
        var obj = await page.GetResourceAsync(ResourceTypeName.ColorSpace, colorSpaceName).CA();
        return obj is PdfArray array? await FromArray(array).CA(): DeviceGray.Instance;
    }

    private async ValueTask<IColorSpace> FromArray(PdfArray array) =>
        await FromMemory((await array.AsAsync<PdfObject>().CA()).AsMemory()).CA();

    private ValueTask<IColorSpace> FromMemory(Memory<PdfObject> memory)
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
            KnownNameKeys.Pattern => FromMemory(memory.Slice(1)),
            var other => SpacesWithoutParameters(other)
        };
    }

    private ValueTask<IColorSpace> SpacesWithoutParameters(int nameHashCode) => nameHashCode switch
    {
        // for monitors ignore CalRGB see standard section 8.6.5.7
        KnownNameKeys.CalRGB => new(DeviceRgb.Instance),
        KnownNameKeys.CalCMYK => CreateCmykColorSpaceAsync(), // standard section 8.6.5.1
        KnownNameKeys.DeviceGray => SearchForDefault(KnownNames.DefaultGray, static ()=>new(DeviceGray.Instance)),
        KnownNameKeys.DeviceRGB => SearchForDefault(KnownNames.DefaultRGB, static ()=>new(DeviceRgb.Instance)),
        KnownNameKeys.DeviceCMYK =>  SearchForDefault(KnownNames.DefaultCMYK, CreateCmykColorSpaceAsync),
        _ => throw new PdfParseException("Unrecognized Colorspace")
    };
        

    private static T ColorSpaceParameterAs<T>(in ReadOnlySpan<PdfObject> array) where T:PdfObject =>
        array[1] as T ?? throw new PdfParseException("Dictionary Expected");
}