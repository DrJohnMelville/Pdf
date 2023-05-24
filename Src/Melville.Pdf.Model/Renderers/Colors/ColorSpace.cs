using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Model;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors.Profiles;

namespace Melville.Pdf.Model.Renderers.Colors;

[StaticSingleton]
internal partial class NoPageContext : IHasPageAttributes
{
    public PdfDictionary LowLevel => PdfDictionary.Empty;
    public ValueTask<Stream> GetContentBytesAsync() => new(new MemoryStream(Array.Empty<byte>()));

    public ValueTask<IHasPageAttributes?> GetParentAsync() => new((IHasPageAttributes?)null);
}

internal readonly struct ColorSpaceFactory
{
    private readonly IHasPageAttributes page;
    public ColorSpaceFactory(IHasPageAttributes page)
    {
        this.page = page;
    }

    public ValueTask<IColorSpace> ParseColorSpaceAsync(PdfName colorSpaceName)
    {
        var code = colorSpaceName.GetHashCode();
        return code switch
        {
            KnownNameKeys.DeviceGray  or KnownNameKeys.DeviceRGB or KnownNameKeys.DeviceCMYK => 
                SpacesWithoutParametersAsync(code),
            _ => LookupInResourceDictionaryAsync(colorSpaceName)
        };
    }

    private async ValueTask<IColorSpace> SearchForDefaultAsync(PdfName name, Func<ValueTask<IColorSpace>> space) =>
        await (await page.GetResourceAsync(ResourceTypeName.ColorSpace, name).CA() is PdfArray array
            ?  ExcludeIllegalDefaultAsync(array): space()).CA();

    private async ValueTask<IColorSpace> ExcludeIllegalDefaultAsync(PdfArray array) => 
        (await FromArrayAsync(array).CA()).AsValidDefaultColorSpace();

    public ValueTask<IColorSpace> FromNameOrArrayAsync(PdfObject datum) => datum switch
    {
        PdfName name => ParseColorSpaceAsync(name),
        PdfArray array => FromArrayAsync(array),
        _ => throw new PdfParseException("Invalid Color space definition")
    };

    private static IColorSpace? cmykColorSpacel;

    public static async ValueTask<IColorSpace> CreateCmykColorSpaceAsync() => cmykColorSpacel ??= 
        new IccColorspaceWithBlackDefault( (await CmykIccProfile.ReadCmykProfileAsync().CA()).DeviceToSrgb());

    private async ValueTask<IColorSpace> LookupInResourceDictionaryAsync(PdfName colorSpaceName)
    {
        var obj = await page.GetResourceAsync(ResourceTypeName.ColorSpace, colorSpaceName).CA();
        return obj is PdfArray array? await FromArrayAsync(array).CA(): DeviceGray.Instance;
    }

    private async ValueTask<IColorSpace> FromArrayAsync(PdfArray array) =>
        await FromMemoryAsync((await array.AsAsync<PdfObject>().CA()).AsMemory()).CA();

    private ValueTask<IColorSpace> FromMemoryAsync(Memory<PdfObject> memory)
    {
        var array = memory.Span;
        if (array.Length == 0) return new(DeviceRgb.Instance);
        if (array.Length == 1 && array[0] is PdfArray innerArray) return FromArrayAsync(innerArray);
        if (array[0] is not PdfName name) throw new PdfParseException("'Name expected in colorspace array");
        return name.GetHashCode() switch
        {
            KnownNameKeys.CalGray => CalGray.ParseAsync(ColorSpaceParameterAs<PdfDictionary>(array)),
            KnownNameKeys.Lab => LabColorSpace.ParseAsync(ColorSpaceParameterAs<PdfDictionary>(array)),
            KnownNameKeys.ICCBased => IccProfileColorSpaceParser.ParseAsync(ColorSpaceParameterAs<PdfStream>(array)),
            KnownNameKeys.Indexed => IndexedColorSpace.ParseAsync(memory, page),
            KnownNameKeys.Separation => SeparationParser.ParseSeparationAsync(memory, page),
            KnownNameKeys.DeviceN => SeparationParser.ParseDeviceNAsync(memory, page),
            KnownNameKeys.Pattern => FromMemoryAsync(memory.Slice(1)),
            var other => SpacesWithoutParametersAsync(other)
        };
    }

    private ValueTask<IColorSpace> SpacesWithoutParametersAsync(int nameHashCode) => nameHashCode switch
    {
        // for monitors ignore CalRGB see standard section 8.6.5.7
        KnownNameKeys.CalRGB => new(DeviceRgb.Instance),
        KnownNameKeys.CalCMYK => CreateCmykColorSpaceAsync(), // standard section 8.6.5.1
        KnownNameKeys.DeviceGray => SearchForDefaultAsync(KnownNames.DefaultGray, static ()=>new(DeviceGray.Instance)),
        KnownNameKeys.DeviceRGB => SearchForDefaultAsync(KnownNames.DefaultRGB, static ()=>new(DeviceRgb.Instance)),
        KnownNameKeys.DeviceCMYK =>  SearchForDefaultAsync(KnownNames.DefaultCMYK, CreateCmykColorSpaceAsync),
        _ => throw new PdfParseException("Unrecognized Colorspace")
    };
        

    private static T ColorSpaceParameterAs<T>(in ReadOnlySpan<PdfObject> array) where T:PdfObject =>
        array[1] as T ?? throw new PdfParseException("Dictionary Expected");
}