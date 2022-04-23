using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.Colors;

public record struct DoubleColor(double Red, double Green, double Blue)
{
    public static async ValueTask<DoubleColor> ParseAsync(PdfArray src)=> new (
            (await src.GetAsync<PdfNumber>(0).CA()).DoubleValue,
            (await src.GetAsync<PdfNumber>(1).CA()).DoubleValue,
            (await src.GetAsync<PdfNumber>(2).CA()).DoubleValue
        );
}

public record struct FloatColor(float Red, float Green, float Blue)
{
    public static implicit operator FloatColor(DoubleColor col) =>
        new((float)col.Red, (float)col.Green, (float)col.Blue);
    public static implicit operator FloatColor(XyzNumber col) =>
        new(col.X, col.Y, col.Z);
}
