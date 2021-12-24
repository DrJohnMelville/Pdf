using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Documents;

public record class PdfPageParent(PdfDictionary LowLevel) : IHasPageAttributes
{
    public virtual ValueTask<Stream> GetContentBytes() => new(new MemoryStream());
    
    public async ValueTask<IHasPageAttributes?> GetParentAsync() =>
        LowLevel.TryGetValue(KnownNames.Parent, out var parentTask) &&
        await parentTask is PdfDictionary dict
            ? new PdfPageParent(dict)
            : null;
}