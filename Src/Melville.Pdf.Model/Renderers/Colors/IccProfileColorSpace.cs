using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Model;
using Melville.Icc.Parser;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.Colors;

public static class IccProfileColorSpace
{
    public static async ValueTask<IColorSpace> ParseAsync(PdfStream getAsync) =>
        await ParseAsync(await getAsync.StreamContentAsync().CA()).CA();
    
    public static async ValueTask<IColorSpace> ParseAsync(Stream source)
    {
        var profile = await new IccParser(PipeReader.Create(source)).ParseAsync().CA();
        return new IccColorSpace(profile.DeviceToSrgb());
    }
}