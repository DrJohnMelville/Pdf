using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.Patterns.TilePatterns;

public record struct 
    TileBrushRequest(
        PdfTilePattern TilePattern,
        Matrix3x2 PatternTransform,
        PdfRect BoundingBox,
        Vector2 RepeatSize,
        int TilePatternType)
{
    public static async ValueTask<TileBrushRequest> Parse(PdfDictionary dict)
    {
        var pdfPattern = new PdfTilePattern(dict);
        var patternTransform = await pdfPattern.Matrix().CA();
        var boundingBox = (await pdfPattern.BBox().CA());
        var repeatSize = new Vector2(
            (float)await pdfPattern.XStep().CA(), (float)await pdfPattern.YStep().CA());
        var patternType = await pdfPattern.PaintType().CA();
        return new(pdfPattern, patternTransform, boundingBox, repeatSize, patternType);
    }
}