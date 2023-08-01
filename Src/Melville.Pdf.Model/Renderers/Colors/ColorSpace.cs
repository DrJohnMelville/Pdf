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
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.Model.Renderers.Colors;

[StaticSingleton]
internal partial class NoPageContext : IHasPageAttributes
{
    public PdfValueDictionary LowLevel => PdfValueDictionary.Empty;
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

    public ValueTask<IColorSpace> ParseColorSpaceAsync(PdfDirectValue colorSpaceName)
    {
        var code = colorSpaceName;
        return code.Equals(KnownNames.DeviceGrayTName) ||
               code.Equals(KnownNames.DeviceRGBTName) ||
               code.Equals(KnownNames.DeviceCMYKTName)
            ? SpacesWithoutParametersAsync(code)
            : LookupInResourceDictionaryAsync(colorSpaceName);
    }

    private async ValueTask<IColorSpace> SearchForDefaultAsync(PdfDirectValue name, Func<ValueTask<IColorSpace>> space) =>
        await ((await page.GetResourceAsync(ResourceTypeName.ColorSpace, name).CA()).TryGet(out PdfValueArray? array) 
            ?  ExcludeIllegalDefaultAsync(array): space()).CA();

    private async ValueTask<IColorSpace> ExcludeIllegalDefaultAsync(PdfValueArray array) => 
        (await FromArrayAsync(array).CA()).AsValidDefaultColorSpace();

    public ValueTask<IColorSpace> FromNameOrArrayAsync(PdfDirectValue datum) => datum switch
    {
        {IsName:true} name => ParseColorSpaceAsync(name),
        var x when x.TryGet(out PdfValueArray? array) => FromArrayAsync(array),
        _ => throw new PdfParseException("Invalid Color space definition")
    };

    private static IColorSpace? cmykColorSpacel;

    public static async ValueTask<IColorSpace> CreateCmykColorSpaceAsync() => cmykColorSpacel ??= 
        new IccColorspaceWithBlackDefault( (await CmykIccProfile.ReadCmykProfileAsync().CA()).DeviceToSrgb());

    private async ValueTask<IColorSpace> LookupInResourceDictionaryAsync(PdfDirectValue colorSpaceName)
    {
        var obj = await page.GetResourceAsync(ResourceTypeName.ColorSpace, colorSpaceName).CA();
        return obj.TryGet(out PdfValueArray? array)? await FromArrayAsync(array).CA(): DeviceGray.Instance;
    }

    private async ValueTask<IColorSpace> FromArrayAsync(PdfValueArray array) =>
        await FromMemoryAsync((await array.AsDirectValues().CA()).AsMemory()).CA();

    private ValueTask<IColorSpace> FromMemoryAsync(Memory<PdfDirectValue> memory)
    {
        var array = memory.Span;
        if (array.Length == 0) return new(DeviceRgb.Instance);
        if (array.Length == 1 && array[0].TryGet(out PdfValueArray innerArray)) 
            return FromArrayAsync(innerArray);
        if (array[0] is not {IsName:true} name) 
            throw new PdfParseException("'Name expected in colorspace array");
        return name switch
        {
            var x when x.Equals(KnownNames.CalGrayTName) => 
                CalGray.ParseAsync(ColorSpaceParameterAs<PdfValueDictionary>(array)),
            var x when x.Equals(KnownNames.LabTName) => 
                LabColorSpace.ParseAsync(ColorSpaceParameterAs<PdfValueDictionary>(array)),
            var x when x.Equals(KnownNames.ICCBasedTName) => 
                IccProfileColorSpaceParser.ParseAsync(ColorSpaceParameterAs<PdfValueStream>(array)),
            var x when x.Equals(KnownNames.IndexedTName) => 
                IndexedColorSpace.ParseAsync(memory, page),
            var x when x.Equals(KnownNames.SeparationTName) => 
                SeparationParser.ParseSeparationAsync(memory, page),
            var x when x.Equals(KnownNames.DeviceNTName) => 
                SeparationParser.ParseDeviceNAsync(memory, page),
            var x when x.Equals(KnownNames.PatternTName) => FromMemoryAsync(memory.Slice(1)),
            var other => SpacesWithoutParametersAsync(other)
        };
    }

    private ValueTask<IColorSpace> SpacesWithoutParametersAsync(PdfDirectValue nameHashCode) => 
        nameHashCode switch
    {
        // for monitors ignore CalRGB see standard section 8.6.5.7
        var x when x.Equals(KnownNames.CalRGBTName) => new(DeviceRgb.Instance),
        var x when x.Equals(KnownNames.CalCMYKTName) => 
            CreateCmykColorSpaceAsync(), // standard section 8.6.5.1
        var x when x.Equals(KnownNames.DeviceGrayTName) => 
            SearchForDefaultAsync(KnownNames.DefaultGrayTName, static ()=>new(DeviceGray.Instance)),
        var x when x.Equals(KnownNames.DeviceRGBTName) => 
            SearchForDefaultAsync(KnownNames.DefaultRGBTName, static ()=>new(DeviceRgb.Instance)),
        var x when x.Equals(KnownNames.DeviceCMYKTName) =>  
            SearchForDefaultAsync(KnownNames.DefaultCMYKTName, CreateCmykColorSpaceAsync),
        _ => throw new PdfParseException("Unrecognized Colorspace")
    };
        

    private static T ColorSpaceParameterAs<T>(in Span<PdfDirectValue> array) =>
        array[1].TryGet(out T ret)? ret: throw new PdfParseException("Dictionary Expected");
}