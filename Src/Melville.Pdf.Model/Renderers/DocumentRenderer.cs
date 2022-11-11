using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.OptionalContents;
using Melville.Pdf.Model.Renderers.Patterns.TilePatterns;

namespace Melville.Pdf.Model.Renderers;

public abstract class DocumentRenderer: IDisposable
{
    public int TotalPages { get; }
    public IDefaultFontMapper FontMapper { get; }
    public IDocumentPartCache Cache { get; }
    public IOptionalContentState OptionalContentState { get; }

    public  DocumentRenderer(int totalPages, 
        IDefaultFontMapper fontMapper, IDocumentPartCache cache, IOptionalContentState optionalContentState)
    {
        this.FontMapper = fontMapper;
        this.Cache = cache;
        OptionalContentState = optionalContentState;
        TotalPages = totalPages;
    }

    public DocumentRenderer PatternRenderer(in TileBrushRequest request, GraphicsState priorState) => 
        new PatternRenderer(FontMapper, Cache, request, priorState, OptionalContentState);

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
        new(page, target, this, new OptionalContentCounter(OptionalContentState));

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

    public virtual void Dispose()
    {
        Cache.Dispose();
    }
}