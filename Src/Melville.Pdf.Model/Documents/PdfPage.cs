using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

namespace Melville.Pdf.Model.Documents;

public record class PdfPage(PdfDictionary LowLevel) : PdfPageParent(LowLevel)
{
    
    public async ValueTask<PdfTime?> LastModifiedAsync()
    {
        return LowLevel.TryGetValue(KnownNames.LastModified, out var task) &&
               await task is PdfString str
            ? str.AsPdfTime()
            : null;
    }

    public override async ValueTask<Stream> GetContentBytes() =>
        await LowLevel.GetOrNullAsync(KnownNames.Contents) switch
        {
            PdfStream strm => await strm.StreamContentAsync(),
            PdfArray array => new PdfArrayConcatStream(array),
            var x => new MemoryStream()
        };
}