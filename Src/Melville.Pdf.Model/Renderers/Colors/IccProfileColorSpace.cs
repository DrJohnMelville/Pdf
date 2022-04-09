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

internal class IccProfileColorSpace
{
    public static async ValueTask<IColorSpace> ParseAsync(PdfStream getAsync)
    {
        var profile = await new IccParser(PipeReader.Create(await getAsync.StreamContentAsync().CA())).ParseAsync().CA();
        return new IccColorSpace( 
            profile.Header.ProfileConnectionColorSpace switch
        {
            ColorSpace.XYZ => profile.DeviceToPcsTransform(RenderIntent.Perceptual) is {} devToXyz?
                new CompositeTransform(devToXyz, XyzToDeviceColor.Instance): NullColorTransform.Instance(3),
            ColorSpace.Lab => profile.TransformTo(await IccProfileLibrary.ReadSrgb().CA()),
            var x => throw new PdfParseException("Unsupported profile connection space: "+x)
        });
    }
}