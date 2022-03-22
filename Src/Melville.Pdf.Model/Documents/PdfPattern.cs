using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers;

namespace Melville.Pdf.Model.Documents;

public record class PdfPattern(PdfDictionary LowLevel) : HasRenderableContentStream(LowLevel)
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
}

public record struct 
    TileBrushRequest(
    PdfPattern Pattern,
    Matrix3x2 PatternTransform,
    PdfRect BoundingBox,
    Vector2 RepeatSize)
{
    public static async ValueTask<TileBrushRequest> Parse(PdfDictionary dict)
    {
        var pdfPattern = new PdfPattern(dict);
        var patternTransform = await pdfPattern.Matrix().CA();
        var boundingBox = (await pdfPattern.BBox().CA());
        var repeatSize = new Vector2(
            (float)await pdfPattern.XStep().CA(), (float)await pdfPattern.YStep().CA());
        return new(pdfPattern, patternTransform, boundingBox, repeatSize);
    }
}
