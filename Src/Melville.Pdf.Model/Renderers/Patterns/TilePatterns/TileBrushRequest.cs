using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.Patterns.TilePatterns;

/// <summary>
/// Parses a tile brush dictionary to a request for a tile brush.
/// </summary>
/// <param name="TilePattern">The pattern to paint.</param>
/// <param name="PatternTransform">Transform for the pattern.</param>
/// <param name="BoundingBox">The bounding box for the content cell.</param>
/// <param name="RepeatSize">The size of the grid that tiles.</param>
/// <param name="TilePatternType">Colored or uncolored tile pattern.</param>
public record struct 
    TileBrushRequest(
        PdfTilePattern TilePattern,
        Matrix3x2 PatternTransform,
        PdfRect BoundingBox,
        Vector2 RepeatSize,
        int TilePatternType)
{
    /// <summary>
    /// Parse a PdfDictionary into a TileBrushRequest
    /// </summary>
    /// <param name="dict">The dictionary defining the tile brush</param>
    /// <returns>A structure with many properties of the tile brush.</returns>
    public static async ValueTask<TileBrushRequest> ParseAsync(PdfDictionary dict)
    {
        var pdfPattern = new PdfTilePattern(dict);
        var patternTransform = await pdfPattern.MatrixAsync().CA();
        var boundingBox = (await pdfPattern.BBoxAsync().CA());
        var repeatSize = new Vector2(
            (float)await pdfPattern.XStepAsync().CA(), (float)await pdfPattern.YStepAsync().CA());
        var patternType = await pdfPattern.PaintTypeAsync().CA();
        return new(pdfPattern, patternTransform, boundingBox, repeatSize, patternType);
    }
}