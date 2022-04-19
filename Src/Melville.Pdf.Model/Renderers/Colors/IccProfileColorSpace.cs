using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Icc.Model.Tags;
using Melville.Icc.Parser;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.Colors;

public static class IccProfileColorSpace
{
    public static async ValueTask<IColorSpace> ParseAsync(PdfStream getAsync)
    {
        var profile = await new IccParser(
            PipeReader.Create(await getAsync.StreamContentAsync().CA())).ParseAsync().CA();
        return new IccColorSpace(DeviceToSrgb(profile));
    }

    public static IColorTransform DeviceToSrgb(this IccProfile profile)
    {
        return profile.DeviceToPcsTransform(RenderIntent.Perceptual)?.Concat(
                   PcsToSrgb(profile.Header.ProfileConnectionColorSpace)) ??
               throw new PdfParseException("Cannot find ICC profile");
    }

    private static IColorTransform PcsToSrgb(ColorSpace pcs) => pcs switch
    {
        ColorSpace.XYZ => XyzToDeviceColor.Instance,
        ColorSpace.Lab => LabToXyz.Instance.Concat(XyzToDeviceColor.Instance),
        var x => throw new PdfParseException("Unsupported profile connection space: " + x)
    };
}