using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using SharpFont;

namespace Melville.Pdf.Model.Renderers;

public class PatternRenderer: DocumentRenderer
{
    private readonly TileBrushRequest request;

    public PatternRenderer(IDefaultFontMapper fontMapper, IDocumentPartCache cache, in TileBrushRequest request) : 
        base(1, fontMapper, cache)
    {
        this.request = request;
    }

    protected override ValueTask<HasRenderableContentStream> GetPageContent(int page) => new(request.Pattern);

    public override void InitializeRenderTarget(IRenderTarget innerRenderer, in PdfRect rect, double width, double height,
        in Matrix3x2 transform)
    {
        var bounds = request.BoundingBox;
        innerRenderer.MapUserSpaceToBitmapSpace(bounds, bounds.Width,bounds.Height, 
             InvertYDimension() );
    }

    private Matrix3x2 InvertYDimension() => 
        Matrix3x2.CreateScale(1,-1)*Matrix3x2.CreateTranslation(0,(float)request.BoundingBox.Height);

    public override (int width, int height) ScalePageToRequestedSize(in PdfRect pageSize, Vector2 requestedSize) => 
        ((int)request.RepeatSize.X, (int)request.RepeatSize.Y);
}