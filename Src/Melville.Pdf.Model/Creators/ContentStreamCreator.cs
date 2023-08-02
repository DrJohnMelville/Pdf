using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

/// <summary>
/// Creates objects which will eventually be stored in a content stream
/// </summary>
public abstract class ContentStreamCreator: ItemWithResourceDictionaryCreator
{
    private readonly IObjectStreamCreationStrategy objStreamStrategy;
    
    private protected ContentStreamCreator(IObjectStreamCreationStrategy objStreamStrategy): base(new())
    {
        this.objStreamStrategy = objStreamStrategy;
    }

    /// <inheritdoc />
    public override (PdfIndirectObject Reference, int PageCount) ConstructItem(IPdfObjectCreatorRegistry creator,
        PdfIndirectObject parent)
    {
        using var _ = objStreamStrategy.EnterObjectStreamContext(creator);
        TryAddResources(creator);
        return (CreateFinalObject(creator), 1);
    }

    /// <summary>
    /// Create the intended object after the resource dictionary is build
    /// </summary>
    /// <param name="creator">PdfObjectRegistry to create new indirect objects.</param>
    /// <returns>Reference to the created object.</returns>
    protected abstract PdfIndirectObject CreateFinalObject(IPdfObjectCreatorRegistry creator);

    /// <summary>
    /// Add a content stream to the build object.
    /// </summary>
    /// <param name="builder">The DictionarBuilder from which to create the content stream.</param>
    /// <param name="data">The data comprising the content stream.</param>
    public abstract void AddToContentStream(DictionaryBuilder builder, MultiBufferStreamSource data);

}