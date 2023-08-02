using System.Threading.Tasks;
using Melville.Icc.ColorTransforms;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.Colors;

internal static class DoubleColorOperations
{
    public static async ValueTask<DoubleColor> AsDoubleColorAsync(this PdfArray src)=> new (
        await src.GetAsync<double>(0).CA(),
        await src.GetAsync<double>(1).CA(),
        await src.GetAsync<double>(2).CA()
    );
}