using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
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
public partial class ContentStreamPreviewRenderer : DocumentRenderer
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
    protected override ValueTask<HasRenderableContentStream> GetPageContentAsync(int oneBasedPageNumber) => 
        ValueTask.FromResult<HasRenderableContentStream>(new ExplicitHRCS(content));

    private partial class ExplicitHRCS : HasRenderableContentStream
    {
        public ExplicitHRCS(Stream content) : base(PdfValueDictionary.Empty)
        {
            Content = content;
        }

        public Stream Content { get; }
        public override ValueTask<Stream> GetContentBytesAsync() => ValueTask.FromResult(Content);
    }
}