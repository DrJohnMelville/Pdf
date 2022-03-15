using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.Model.Renderers;

public sealed class DocumentRenderer
{
    private readonly Func<long, ValueTask<HasRenderableContentStream>> pageSource;
    public int TotalPages { get; }
    public IDefaultFontMapper FontMapper { get; }
    public IDocumentPartCache Cache { get; }

    public DocumentRenderer(
        Func<long, ValueTask<HasRenderableContentStream>> pageSource, int totalPages,
        IDefaultFontMapper fontMapper) : this(pageSource, totalPages, fontMapper, new DocumentPartCache())
    {
        
    }
    private DocumentRenderer(
        Func<long, ValueTask<HasRenderableContentStream>> pageSource, int totalPages, 
        IDefaultFontMapper fontMapper, IDocumentPartCache cache)
    {
        this.pageSource = pageSource;
        this.FontMapper = fontMapper;
        this.Cache = cache;
        TotalPages = totalPages;
    }

    public DocumentRenderer SubRenderer(HasRenderableContentStream item) => 
        new DocumentRenderer(_ => ValueTask.FromResult(item), 1, FontMapper);

    public async ValueTask RenderPageTo(int page, Func<PdfRect, Matrix3x2, IRenderTarget> target)
    {
        var pageStruct = await pageSource(page).CA();
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
        new(page, target, this);
}

public static class DocumentRendererFactory
{
    public static DocumentRenderer CreateRenderer(
        HasRenderableContentStream page, IDefaultFontMapper fontFactory) =>
        new(_ => new(page), 1, fontFactory);
    
    public static async ValueTask<DocumentRenderer> CreateRendererAsync(
        PdfDocument document, IDefaultFontMapper fontFactory)
    {
        var pages = await document.PagesAsync().CA();
        var pageCount = (int)await pages.CountAsync().CA();
        return new DocumentRenderer( pages.GetPageAsync, pageCount, fontFactory);
    }
}