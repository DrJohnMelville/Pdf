using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.Model.Documents;

/// <summary>
/// This record is the primary abstraction for a page in a pdf document.
/// </summary>
/// <param name="LowLevel"></param>
public record class PdfPage(PdfValueDictionary LowLevel) : HasRenderableContentStream(LowLevel)
{
    /// <summary>
    /// Get the last modified time for a page from the page's dictionary
    /// </summary>
    /// <returns>The last modified time if it exists, null otherwise.</returns>
    public async ValueTask<PdfTime?> LastModifiedAsync()
    {
        return (await LowLevel.GetOrNullAsync(KnownNames.LastModifiedTName).CA()) is {IsName:false} str
            ? ParseToDateTime(str)
            : null;
    }

    private static PdfTime ParseToDateTime(PdfDirectValue str) => 
        new PdfTimeParser(str.Get<StringSpanSource>().GetSpan()).AsPdfTime();

    /// <summary>
    /// Gets a C# stream that represents the content stream for this page
    /// </summary>
    public override async ValueTask<Stream> GetContentBytesAsync() =>
        await LowLevel.GetOrNullAsync(KnownNames.ContentsTName).CA() switch
        {
            var x when x.TryGet(out PdfValueStream? strm) => await strm.StreamContentAsync().CA(),
            var x when x.TryGet(out PdfValueArray? array) => new PdfArrayConcatStream(array),
            var x => new MemoryStream()
        };
}