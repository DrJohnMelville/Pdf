using System.Threading.Tasks;
using Melville.Icc.ColorTransforms;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.Model.Renderers.Colors;

internal static class DoubleColorOperations
{
    public static async ValueTask<DoubleColor> AsDoubleColorAsync(this PdfValueArray src)=> new (
        await src.GetAsync<double>(0).CA(),
        await src.GetAsync<double>(1).CA(),
        await src.GetAsync<double>(2).CA()
    );
}