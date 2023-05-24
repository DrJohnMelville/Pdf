using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

namespace Melville.Pdf.Model.Documents;

/// <summary>
/// This record is the primary abstraction for a page in a pdf document.
/// </summary>
/// <param name="LowLevel"></param>
public record class PdfPage(PdfDictionary LowLevel) : HasRenderableContentStream(LowLevel)
{
    /// <summary>
    /// Get the last modified time for a page from the page's dictionary
    /// </summary>
    /// <returns>The last modified time if it exists, null otherwise.</returns>
    public async ValueTask<PdfTime?> LastModifiedAsync()
    {
        return LowLevel.TryGetValue(KnownNames.LastModified, out var task) &&
               await task.CA() is PdfString str
            ? str.AsPdfTime()
            : null;
    }

    /// <summary>
    /// Gets a C# stream that represents the content stream for this page
    /// </summary>
    public override async ValueTask<Stream> GetContentBytesAsync() =>
        await LowLevel.GetOrNullAsync(KnownNames.Contents).CA() switch
        {
            PdfStream strm => await strm.StreamContentAsync().CA(),
            PdfArray array => new PdfArrayConcatStream(array),
            var x => new MemoryStream()
        };
}