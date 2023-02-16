using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.Model.Renderers.DocumentRenderers;

internal class OwnedPageTreeDocumentRenderer : PageTreeDocumentRenderer
{
    private readonly PdfDocument document;
    public OwnedPageTreeDocumentRenderer(
        int totalPages, IDefaultFontMapper fontMapper, IDocumentPartCache cache, PageTree tree, 
        IOptionalContentState ocs, PdfDocument document) : base(totalPages, fontMapper, cache, tree, ocs)
    {
        this.document = document;
    }

    public override void Dispose()
    {
        document.Dispose();
        base.Dispose();
    }
}