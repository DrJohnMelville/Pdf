using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.Model.Renderers.DocumentRenderers;

/// <summary>
/// This is a document Renderer that will draw a loose content stream with no resources
/// to an IRenderTarget.  It exists so the low level viewer can preview glyphs from
/// Type 3 fonts.
/// </summary>
public class ContentStreamPreviewRenderer : DocumentRenderer
{
    private readonly Stream content;

    /// <summary>
    /// Create a ContentStreamPreviewRenderer
    /// </summary>
    /// <param name="fontMapper">Font library to map built in fonts.</param>
    /// <param name="content">the stream to be rendered.</param>
    public ContentStreamPreviewRenderer(IDefaultFontMapper fontMapper, Stream content) : 
        base(1, fontMapper, new DocumentPartCache(), AllOptionalContentVisible.Instance)
    {
        this.content = content;
    }

    /// <inheritdoc />
    protected override ValueTask<HasRenderableContentStream> GetPageContent(int oneBasedPageNumber) => 
        ValueTask.FromResult<HasRenderableContentStream>(new ExplicitHRCS(content));

    private record ExplicitHRCS(Stream Content) : 
        HasRenderableContentStream(PdfDictionary.Empty)
    {
        public override ValueTask<Stream> GetContentBytes() => ValueTask.FromResult(Content);
    }
}