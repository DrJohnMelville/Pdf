using System.Threading.Tasks;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.Model.Renderers.DocumentRenderers;

internal class PageTreeDocumentRenderer : DocumentRenderer
{
    private readonly PageTree tree;

    public PageTreeDocumentRenderer(int totalPages, IDefaultFontMapper fontMapper, IDocumentPartCache cache, 
        PageTree tree, IOptionalContentState ocs) :
        base(totalPages, fontMapper, cache, ocs)
    {
        this.tree = tree;
    }

    protected override ValueTask<HasRenderableContentStream> GetPageContentAsync(int oneBasedPageNumber) =>
        tree.GetPageAsync(oneBasedPageNumber);
}