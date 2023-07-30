using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.Model.Documents;

/// <summary>
/// This is a base class for costume types PdfPage and PdfTilePattern, both of which
/// have a content stream.  It is implemented as a record, and PdfPageTree actually depends
/// on the equality members
/// </summary>
/// <param name="LowLevel">The low level Dictionary representing this item.</param>
public class HasRenderableContentStream : IHasPageAttributes
{
    public PdfValueDictionary LowLevel { get; }

    public HasRenderableContentStream(PdfValueDictionary lowLevel)
    {
        ArgumentNullException.ThrowIfNull(lowLevel);
        LowLevel = lowLevel;
    }

    /// <summary>
    /// Get a stream representing the content stream for this item.
    /// </summary>
    public virtual ValueTask<Stream> GetContentBytesAsync() => new(new MemoryStream());

    async ValueTask<IHasPageAttributes?> IHasPageAttributes.GetParentAsync() =>
        await LowLevel.GetOrDefaultAsync(
            KnownNames.ParentTName, (PdfValueDictionary?) null).CA() is {} dict
            ? new HasRenderableContentStream(dict)
            : null;
    
    /// <summary>
    /// Get a value indicating how the page should be initially rotated
    /// </summary>
    /// <returns>The desired rotation, in degrees</returns>
    public ValueTask<long> GetDefaultRotationAsync() => 
        LowLevel.GetOrDefaultAsync(KnownNames.RotateTName, 0L);
}