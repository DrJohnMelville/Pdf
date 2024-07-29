using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

namespace Melville.Pdf.Model.Documents;

/// <summary>
/// This record is the primary abstraction for a page in a pdf document.
/// </summary>
[FromConstructor]
public partial class PdfPage: HasRenderableContentStream
{
    /// <summary>
    /// Get the last modified time for a page from the page's dictionary
    /// </summary>
    /// <returns>The last modified time if it exists, null otherwise.</returns>
    public async ValueTask<PdfTime?> LastModifiedAsync()
    {
        return (await LowLevel.GetOrNullAsync(KnownNames.LastModified).CA()) is {IsName:false} str
            ? str.AsPdfTime()
            : null;
    }

    /// <summary>
    /// Gets a C# stream that represents the content stream for this page
    /// </summary>
    public override async ValueTask<Stream> GetContentBytesAsync() =>
        await LowLevel.GetOrNullAsync(KnownNames.Contents).CA() switch
        {
            var x when x.TryGet(out PdfStream? strm) => await strm.StreamContentAsync().CA(),
            var x when x.TryGet(out PdfArray? array) => new PdfArrayConcatStream(array),
            var x => new MemoryStream()
        };
}