using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.Model.DocumentRenderers;

public class DocumentRenderer
{
    private readonly Func<long, ValueTask<PdfPage>> pageSource;
    public int TotalPages { get; }
    private IDefaultFontMapper fontFactory;
    private IDocumentPartCache cache = new DocumentPartCache();

    public DocumentRenderer(Func<long, ValueTask<PdfPage>> pageSource, int totalPages, IDefaultFontMapper fontFactory)
    {
        this.pageSource = pageSource;
        this.fontFactory = fontFactory;
        TotalPages = totalPages;
    }
    
    public async ValueTask RenderPageTo(int page, Func<PdfRect,IRenderTarget> target)
    {
        var pageStruct = await pageSource(page).CA();
        var cropRect = await GetCropDimensionsAsync(pageStruct).CA();
        using var renderTarget = target(cropRect);
        await CreateRenderEngine(pageStruct, renderTarget).RunContentStream().CA();
    }

    private async ValueTask<PdfRect> GetCropDimensionsAsync(PdfPage pageStruct) => 
        await pageStruct.GetBoxAsync(BoxName.CropBox).CA() ?? new PdfRect(0,0,1,1);

    private RenderEngine CreateRenderEngine(in PdfPage page, IRenderTarget target) =>
        new RenderEngine(page, target, fontFactory, cache);

}

public static class DocumentRendererFactory
{
    public static DocumentRenderer CreateRenderer(
        PdfPage page, IDefaultFontMapper fontFactory) =>
        new DocumentRenderer(_ => new(page), 1, fontFactory);
    
    public static async ValueTask<DocumentRenderer> CreateRendererAsync(
        PdfDocument document, IDefaultFontMapper fontFactory)
    {
        var pages = await document.PagesAsync().CA();
        var pageCount = (int)await pages.CountAsync().CA();
        return new DocumentRenderer(pages.GetPageAsync, pageCount, fontFactory);
    }
}