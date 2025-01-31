using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.Patterns.TilePatterns;

/// <summary>
/// This is a costume type that represents wraps a dictionary as a tile pattern.
/// </summary>
[FromConstructor]
public partial class PdfTilePattern: HasRenderableContentStream
{
    /// <inheritdoc />
    public override ValueTask<Stream> GetContentBytesAsync() => ((PdfStream)LowLevel).StreamContentAsync();

    /// <summary>
    /// The horizontal size of the pattern cell
    /// </summary>
    public ValueTask<double> XStepAsync() => LowLevel.GetOrDefaultAsync(KnownNames.XStep, 0.0);
    
    /// <summary>
    /// The vertical size of the pattern cell
    /// </summary>
    public ValueTask<double> YStepAsync() => LowLevel.GetOrDefaultAsync(KnownNames.YStep, 0.0);

    /// <summary>
    /// Bounding box for the pattern cell.
    /// </summary>
    public async ValueTask<PdfRect> BBoxAsync() => await PdfRect.CreateAsync(
        await LowLevel.GetAsync<PdfArray>(KnownNames.BBox).CA()).CA();

    /// <summary>
    /// Patternn matrix transform.
    /// </summary>
    public async ValueTask<Matrix3x2> MatrixAsync() =>
        (await LowLevel.GetOrDefaultAsync(KnownNames.Matrix, PdfArray.Empty).CA()) is {Count:6 } matArray
            ? await matArray.AsMatrix3x2Async().CA()
            : Matrix3x2.Identity;

    /// <summary>
    /// The paint type for the tile pattern.
    /// </summary>
    /// <returns>1 is for a Colored Tile Pattern, and 2 for an Uncolored Tile Pattern</returns>
    public async ValueTask<int> PaintTypeAsync() =>
        (int)(await LowLevel.GetAsync<int>(KnownNames.PaintType).CA());
}