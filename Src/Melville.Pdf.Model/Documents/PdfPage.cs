using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers;

namespace Melville.Pdf.Model.Documents;

public readonly struct PdfPage : IHasPageAttributes
{
    public PdfDictionary LowLevel { get; }

    public PdfPage(PdfDictionary lowLevel)
    {
        LowLevel = lowLevel;
    }

    public async ValueTask<PdfTime?> LastModifiedAsync()
    {
        return LowLevel.TryGetValue(KnownNames.LastModified, out var task) &&
               await task is PdfString str
            ? str.AsPdfTime()
            : null;
    }

    public async ValueTask<Stream> GetContentBytes() =>
        await LowLevel.GetOrNullAsync(KnownNames.Contents) switch
        {
            PdfStream strm => await strm.StreamContentAsync(),
            PdfArray array => new PdfArrayConcatStream(array),
            _ => throw new PdfParseException("Could not find content stream")
        };
}