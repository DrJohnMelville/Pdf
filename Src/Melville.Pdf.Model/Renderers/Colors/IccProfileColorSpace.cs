using System;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Model;
using Melville.Icc.Parser;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.Colors;

public static class IccProfileColorSpace
{
    public static async ValueTask<IColorSpace> ParseAsync(PdfStream stream)
    {
        try
        {
            return await ParseAsync(await stream.StreamContentAsync().CA()).CA();
        }
        catch (Exception)
        {
            return await await ParseAlternateColorSpace(stream).CA();
        }
    }

    private static async Task<ConfiguredValueTaskAwaitable<IColorSpace>> ParseAlternateColorSpace(PdfStream stream) =>
        new ColorSpaceFactory(NoPageContext.Instance)
            .FromNameOrArray(await stream.GetOrDefaultAsync(KnownNames.Alternate,
                DefaultColorSpace(await stream.GetOrDefaultAsync(KnownNames.N, 0).CA())).CA()).CA();

    private static PdfName DefaultColorSpace(long n) =>
        n switch
        {
            1 => KnownNames.DeviceGray,
            3 => KnownNames.DeviceRGB,
            4 => KnownNames.DeviceCMYK,
            _ => throw new PdfParseException("Cannot construct default colorspace")
        };

    public static async ValueTask<IColorSpace> ParseAsync(Stream source)
    {
        var profile = await new IccParser(PipeReader.Create(source)).ParseAsync().CA();
        return new IccColorSpace(profile.DeviceToSrgb());
    }
}