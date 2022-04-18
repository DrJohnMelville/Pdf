using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Icc.Model.Tags;
using Melville.Icc.Parser;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Renderers.Colors.Profiles;

namespace Melville.Pdf.Model.Renderers.Colors;

public static class IccProfileColorSpace
{
    public static async ValueTask<IColorSpace> ParseAsync(PdfStream getAsync)
    {
        var profile = await new IccParser(PipeReader.Create(await getAsync.StreamContentAsync().CA())).ParseAsync().CA();
        return new IccColorSpace(await DeviceToSrgb(profile).CA());
    }

    public static async Task<IColorTransform> DeviceToSrgb(this IccProfile profile)
    {
        return profile.DeviceToPcsTransform(RenderIntent.Perceptual)?.Concat(
                   await PcsToSrgb(profile.Header.ProfileConnectionColorSpace).CA()) ??
               throw new PdfParseException("Cannot find ICC profile");
    }

    private static async ValueTask<IColorTransform> PcsToSrgb(ColorSpace pcs) => pcs switch
    {
        ColorSpace.XYZ => XyzToDeviceColor.Instance,
        ColorSpace.Lab => (await IccProfileLibrary.ReadSrgb().CA()).PcsToDeviceTransform(RenderIntent.Perceptual) ??
                          throw new InvalidOperationException("This profile should exist"),
        var x => throw new PdfParseException("Unsupported profile connection space: " + x)
    };
}