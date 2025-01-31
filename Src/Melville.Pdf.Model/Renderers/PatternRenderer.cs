using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.Patterns.TilePatterns;

namespace Melville.Pdf.Model.Renderers;

internal class PatternRenderer(
    IDefaultFontMapper fontMapper,
    IDocumentPartCache cache,
    in TileBrushRequest request,
    GraphicsState priorState,
    IOptionalContentState ocs)
    : DocumentRenderer(1, fontMapper, cache, ocs)
{
    private readonly TileBrushRequest request = request;

    protected override ValueTask<HasRenderableContentStream> GetPageContentAsync(int oneBasedPageNumber) => 
        new(request.TilePattern);

    public override void InitializeRenderTarget(
        IRenderTarget innerRenderer, in PdfRect rect, double width, double height,
        in Matrix3x2 PageRotationTransform)
    {
        innerRenderer.CloneStateFrom(priorState);
        var bounds = request.BoundingBox;
        innerRenderer.MapUserSpaceToBitmapSpace(bounds, bounds.Width,bounds.Height, 
             InvertYDimension() );
    }

    private Matrix3x2 InvertYDimension() => 
        Matrix3x2.CreateScale(1,-1)*Matrix3x2.CreateTranslation(0,(float)request.BoundingBox.Height);

    public override (int width, int height) ScalePageToRequestedSize(in PdfRect pageSize, Vector2 requestedSize) => 
        ((int)request.RepeatSize.X, (int)request.RepeatSize.Y);

    public override IColorOperations AdjustColorOperationsModel(IColorOperations inner) =>
        request.TilePatternType == 2? 
            ColorOperations.NullColorOperations.Instance : 
            base.AdjustColorOperationsModel(inner);
}