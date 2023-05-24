using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.Model.Renderers.DocumentRenderers;

/// <summary>
/// This class builds a document renderer from a PdfDocument.  It is largely responsible for
/// creating all of the document renderer's state and finding the page tree in the document/
/// </summary>
public static class DocumentRendererFactory
{
    /// <summary>
    /// Create a document render fpr a given page with the given font mapper.  This method exists
    /// for certain programs (like the low level viewer) with significant insight into the pdf
    /// document structure and end up with a PdfPage object that they want to render.  The
    /// vast majority of users should use the CreateRendererAsync method on this class to create
    /// a single renderer for the entire PdfDocument and then render the desired pages from that.
    /// This will ensure that reused elements are cached correctly, and optional content blocks
    /// are evaluated correctly
    /// </summary>
    /// <param name="page">A PDF object (Page or tile pattern) to render.</param>
    /// <param name="fontFactory">A font mapper to realize font definitions used in the content
    /// stream</param>
    /// <returns>A document renderer that can render the given item</returns>
    public static DocumentRenderer CreateRenderer(
        HasRenderableContentStream page, IDefaultFontMapper fontFactory) =>
        new ExplicitDocumentRenderer(fontFactory, new DocumentPartCache(), page,
            AllOptionalContentVisible.Instance);
    
    /// <summary>
    /// Given a PdfDocument and a font mapper, create the DocumentRenderer that can render
    /// pages from that document.
    /// </summary>
    /// <param name="document">The PdfDocument to render</param>
    /// <param name="fontFactory">IDefaultFontMapper to render fonts found within the document.</param>
    /// <returns></returns>
    public static async ValueTask<DocumentRenderer> CreateRendererAsync(
        PdfDocument document, IDefaultFontMapper fontFactory)
    {
        var pages = await document.PagesAsync().CA();
        var pageCount = (int)await pages.CountAsync().CA();
        
        return new OwnedPageTreeDocumentRenderer(pageCount, fontFactory, new DocumentPartCache(), pages,
            await ParseOptionalContentVisibilityAsync(document).CA(), document);
    }

    private static async Task<IOptionalContentState> ParseOptionalContentVisibilityAsync(PdfDocument document) =>
        await OptionalContentPropertiesParser.ParseAsync(
            await document.OptionalContentPropertiesAsync().CA()).CA();
}