using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.Patterns.TilePatterns;

public record PdfTilePattern(PdfDictionary LowLevel) : HasRenderableContentStream(LowLevel)
{
    public override ValueTask<Stream> GetContentBytes() => ((PdfStream)LowLevel).StreamContentAsync();

    public ValueTask<double> XStep() => LowLevel.GetOrDefaultAsync(KnownNames.XStep, 0.0);
    public ValueTask<double> YStep() => LowLevel.GetOrDefaultAsync(KnownNames.XStep, 0.0);
    public async ValueTask<PdfRect> BBox() => await PdfRect.CreateAsync(
        await LowLevel.GetAsync<PdfArray>(KnownNames.BBox).CA()).CA();

    public async ValueTask<Matrix3x2> Matrix() =>
        LowLevel.TryGetValue(KnownNames.Matrix, out var matTask) && await matTask.CA() is PdfArray matArray
            ? await matArray.AsMatrix3x2Async().CA()
            : Matrix3x2.Identity;

    public async ValueTask<int> PaintType() =>
        (int)(await LowLevel.GetAsync<PdfNumber>(KnownNames.PaintType).CA()).IntValue;
}