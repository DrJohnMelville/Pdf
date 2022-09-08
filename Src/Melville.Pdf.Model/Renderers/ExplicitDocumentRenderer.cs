using System.Threading.Tasks;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.Model.Renderers;

public class ExplicitDocumentRenderer : DocumentRenderer
{
    private readonly HasRenderableContentStream content;
    public ExplicitDocumentRenderer(
        IDefaultFontMapper fontMapper, IDocumentPartCache cache, HasRenderableContentStream content,
        IOptionalContentState ocs) : 
        base(1, fontMapper, cache, ocs)
    {
        this.content = content;
    }
    protected override ValueTask<HasRenderableContentStream> GetPageContent(int oneBasedPageNumber) => new(content);
}