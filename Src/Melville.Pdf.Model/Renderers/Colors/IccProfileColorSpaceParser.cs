using System;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Parser;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.Colors;


/// <summary>
/// Parse a ICCProfile from a PdfStream
/// </summary>
public static class IccProfileColorSpaceParser
{
    /// <summary>
    /// Parse a ICC profile from a PDF stream
    /// </summary>
    /// <param name="stream">The CodeSource to read the ICC profile from.</param>
    /// <returns>The ICC colorspace read from the stream.</returns>
    public static async ValueTask<IColorSpace> ParseAsync(PdfStream stream)
    {
        try
        {
            return await ParseAsync(await stream.StreamContentAsync().CA()).CA();
        }
        catch (Exception)
        {
            return await await ParseAlternateColorSpaceAsync(stream).CA();
        }
    }

    private static async Task<ConfiguredValueTaskAwaitable<IColorSpace>> ParseAlternateColorSpaceAsync(PdfStream stream) =>
        new ColorSpaceFactory(NoPageContext.Instance)
            .FromNameOrArrayAsync(await stream.GetOrDefaultAsync(KnownNames.Alternate,
                DefaultColorSpace(await stream.GetOrDefaultAsync(KnownNames.N, 0).CA())).CA()).CA();

    private static PdfDirectObject DefaultColorSpace(long n) =>
        n switch
        {
            1 => KnownNames.DeviceGray,
            3 => KnownNames.DeviceRGB,
            4 => KnownNames.DeviceCMYK,
            _ => throw new PdfParseException("Cannot construct default colorspace")
        };

    /// <summary>
    /// Parse an ICC colorspace from a C@ Stream
    /// </summary>
    /// <param name="source">C# stream to read the profile from.</param>
    /// <returns>Colorspace using the ICC profile.</returns>
    public static async ValueTask<IColorSpace> ParseAsync(Stream source)
    {
        var profile = await new IccParser(PipeReader.Create(source)).ParseAsync().CA();
        return new IccColorSpace(profile.DeviceToSrgb());
    }
}