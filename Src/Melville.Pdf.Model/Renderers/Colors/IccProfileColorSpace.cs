using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Icc.Parser;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Renderers.Colors.Profiles;

namespace Melville.Pdf.Model.Renderers.Colors;

internal class IccProfileColorSpace
{
    public static async Task<IColorSpace> ParseAsync(PdfStream getAsync)
    {
        var profile = await new IccParser(PipeReader.Create(await getAsync.StreamContentAsync())).ParseAsync();
        return new IccColorSpace( 
            profile.Header.ProfileConnectionColorSpace switch
        {
            ColorSpace.XYZ => new CompositeTransform(profile.DeviceToPcsTransform(RenderIntent.Perceptual)??
                                                     throw new PdfParseException("Invalid icc profile"),
                XyzToDeviceColor.Instance),
            ColorSpace.Lab => profile.TransformTo(await IccProfileLibrary.ReadSrgb()),
            var x => throw new PdfParseException("Unsupported profile connection space: "+x)
        });
    }
}