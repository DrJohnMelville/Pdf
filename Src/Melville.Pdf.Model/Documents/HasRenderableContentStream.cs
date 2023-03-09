using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Documents;

/// <summary>
/// This is a base class for costume types PdfPage and PdfTilePattern, both of which
/// have a content stream.  It is implemented as a record, and PdfPageTree actually depends
/// on the equality members
/// </summary>
/// <param name="LowLevel">The low level Dictionary representing this item.</param>
public record class HasRenderableContentStream(PdfDictionary LowLevel) : IHasPageAttributes
{
    /// <summary>
    /// Get a stream representing the content stream for this item.
    /// </summary>
    public virtual ValueTask<Stream> GetContentBytes() => new(new MemoryStream());

    async ValueTask<IHasPageAttributes?> IHasPageAttributes.GetParentAsync() =>
        LowLevel.TryGetValue(KnownNames.Parent, out var parentTask) &&
        await parentTask.CA() is PdfDictionary dict
            ? new HasRenderableContentStream(dict)
            : null;
    
    /// <summary>
    /// Get a value indicating how the page should be initially rotated
    /// </summary>
    /// <returns>The desired rotation, in degrees</returns>
    public ValueTask<long> GetDefaultRotationAsync() => 
        LowLevel.GetOrDefaultAsync(KnownNames.Rotate, 0);
}