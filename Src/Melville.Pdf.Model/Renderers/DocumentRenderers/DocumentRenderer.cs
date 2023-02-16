using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.OptionalContents;
using Melville.Pdf.Model.Renderers.Patterns.TilePatterns;

namespace Melville.Pdf.Model.Renderers.DocumentRenderers;

/// <summary>
/// This class represents the state of the renderer, which may be refused between pages. 
/// </summary>
public abstract class DocumentRenderer: IDisposable
{
    /// <summary>
    /// Total pages available to render
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// Maps font names to fonts.
    /// </summary>
    public IDefaultFontMapper FontMapper { get; }

    /// <summary>
    /// Cache for partially done work that may be valuable later on.  (Es
    /// </summary>
    internal IDocumentPartCache Cache { get; }

    /// <summary>
    /// Records the visibility of various optional content group.
    /// </summary>
    public IOptionalContentState OptionalContentState { get; }

    internal DocumentRenderer(int totalPages, 
        IDefaultFontMapper fontMapper, IDocumentPartCache cache, IOptionalContentState optionalContentState)
    {
        this.FontMapper = fontMapper;
        this.Cache = cache;
        OptionalContentState = optionalContentState;
        TotalPages = totalPages;
    }

    /// <summary>
    /// Create a DocumentRenderer that will render a tile pattern from this document.
    /// </summary>
    /// <param name="request">The tile brush to render.</param>
    /// <param name="priorState">The graphics state at the time of the tile brush request.</param>
    /// <returns>The DocumentReader that can render the TilePatternRequest.</returns>
    public DocumentRenderer PatternRenderer(in TileBrushRequest request, GraphicsState priorState) => 
        new PatternRenderer(FontMapper, Cache, request, priorState, OptionalContentState);

    /// <summary>
    /// Render a given page to a target.
    /// </summary>
    /// <param name="oneBasedPageNumber">The page to render</param>
    /// <param name="target">A factory method that creates a render target given a visible rectangle and matrix.</param>
    /// <returns></returns>
    public async ValueTask RenderPageTo(int oneBasedPageNumber, Func<PdfRect, Matrix3x2, IRenderTarget> target)
    {
        var pageStruct = await GetPageContent(oneBasedPageNumber).CA();
        var cropRect = await GetCropDimensionsAsync(pageStruct).CA();
        var rotation = CreateRotateMatrix( cropRect, await pageStruct.GetDefaultRotationAsync().CA());
        
        using var renderTarget = target(cropRect.Transform(rotation),rotation);
        await CreateRenderEngine(pageStruct, renderTarget).RunContentStream().CA();
    }

    private Matrix3x2 CreateRotateMatrix(PdfRect cropRect, long rotateBy)
    {
        var centerX = cropRect.Left + cropRect.Right / 2;
        var centerY = cropRect.Top + cropRect.Bottom / 2;
        return Matrix3x2.CreateRotation((float) (rotateBy * Math.PI / -180), 
            new Vector2((float)centerX, (float)centerY));
    }

    private async ValueTask<PdfRect> GetCropDimensionsAsync(IHasPageAttributes pageStruct) => 
        await pageStruct.GetBoxAsync(BoxName.CropBox).CA() ?? new PdfRect(0,0,1,1);

    private RenderEngine CreateRenderEngine(HasRenderableContentStream page, IRenderTarget target) =>
        new(page, new (target, this, OptionalContentEngine()));

    private IOptionalContentCounter OptionalContentEngine()
    {
        return OptionalContentState.AllVisible() ? 
            NullOptionalContentCounter.Instance:
            new OptionalContentCounter(OptionalContentState);
    }

    public virtual void InitializeRenderTarget(IRenderTarget innerRenderer,
        in PdfRect rect, double width, double height, in Matrix3x2 transform)
    {
        innerRenderer.GraphicsState.CurrentState().SetPageSize(width, height);
        innerRenderer.SetBackgroundRect(rect, width, height, transform);
        innerRenderer.MapUserSpaceToBitmapSpace(rect, width, height, transform);
    }

    public virtual (int width, int height) ScalePageToRequestedSize(in PdfRect pageSize, Vector2 requestedSize)=>
        (requestedSize) switch
        {
            { X: < 0, Y:< 0 } => ((int)pageSize.Width, (int)pageSize.Height),
            {X: < 0} => new(Scale(pageSize.Width, requestedSize.Y, pageSize.Height), (int)requestedSize.Y),
            {Y: < 0} => new((int)requestedSize.X, Scale(pageSize.Height, requestedSize.X, pageSize.Width)),
            _ => ((int)requestedSize.X, (int)requestedSize.Y)
        };
    
    private static int Scale(double freeDimension, float setValue, double setDimension) => 
        (int)(freeDimension * (setValue / setDimension));

    protected abstract ValueTask<HasRenderableContentStream> GetPageContent(int oneBasedPageNumber);

    public virtual void Dispose() => Cache.Dispose();
    
    public virtual IColorOperations AdjustColorOperationsModel(IColorOperations inner) => inner;
}