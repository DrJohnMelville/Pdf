using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.Model.Renderers.DocumentRenderers;

public static class DocumentRendererFactory
{
    public static DocumentRenderer CreateRenderer(
        HasRenderableContentStream page, IDefaultFontMapper fontFactory) =>
        new ExplicitDocumentRenderer(fontFactory, new DocumentPartCache(), page,
            AllOptionalContentVisible.Instance);
    
    public static async ValueTask<DocumentRenderer> CreateRendererAsync(
        PdfDocument document, IDefaultFontMapper fontFactory)
    {
        var pages = await document.PagesAsync().CA();
        var pageCount = (int)await pages.CountAsync().CA();
        
        return new OwnedPageTreeDocumentRenderer(pageCount, fontFactory, new DocumentPartCache(), pages,
            await ParseOptionalContentVisibility(document).CA(), document);
    }

    private static async Task<IOptionalContentState> ParseOptionalContentVisibility(PdfDocument document) =>
        await OptionalContentPropertiesParser.ParseAsync(
            await document.OptionalContentProperties().CA()).CA();
}