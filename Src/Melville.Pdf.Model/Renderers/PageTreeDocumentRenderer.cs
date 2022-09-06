using System.Threading.Tasks;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.Model.Renderers;

public class PageTreeDocumentRenderer : DocumentRenderer
{
    private readonly PageTree tree;

    public PageTreeDocumentRenderer(int totalPages, IDefaultFontMapper fontMapper, IDocumentPartCache cache, 
        PageTree tree, IOptionalContentState ocs) :
        base(totalPages, fontMapper, cache, ocs)
    {
        this.tree = tree;
    }

    protected override ValueTask<HasRenderableContentStream> GetPageContent(int page) =>
        tree.GetPageAsync(page);
}