﻿using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Parser;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.PipeReaders;
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
        var altName = await ComputeAlternateColorSpaceNameAsync(stream).CA();
        try
        {
            if (!HasExplicitAlternateRgbColorSpace(stream, altName))
            {
                await using var content = await stream.StreamContentAsync().CA();
                return await ParseAsync(content).CA();
            }
        }
        catch (Exception)
        {
        }
        return await ParseAlternateColorSpaceAsync(altName).CA();
    }

    private static bool HasExplicitAlternateRgbColorSpace(PdfStream stream, PdfDirectObject altName) => 
        altName.Equals(KnownNames.DeviceRGB) && stream.ContainsKey(KnownNames.Alternate);

    private static async ValueTask<PdfDirectObject> ComputeAlternateColorSpaceNameAsync(PdfStream stream) =>
        await stream.GetOrDefaultAsync(KnownNames.Alternate,
            DefaultColorSpace(await stream.GetOrDefaultAsync(KnownNames.N, 0).CA())).CA();

    private static ValueTask<IColorSpace> ParseAlternateColorSpaceAsync(PdfDirectObject altName) =>
        new ColorSpaceFactory(NoPageContext.Instance)
            .FromNameOrArrayAsync(altName);

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
        using var reader = MultiplexSourceFactory.SingleReaderForStream(source, false);
        var profile = await new IccParser(reader)
            .ParseAsync().CA();
        return new IccColorSpace(profile.DeviceToSrgb());
    }
}