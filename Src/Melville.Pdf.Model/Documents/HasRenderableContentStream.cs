using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Documents;

public record class HasRenderableContentStream(PdfDictionary LowLevel) : IHasPageAttributes
{
    public virtual ValueTask<Stream> GetContentBytes() => new(new MemoryStream());
    
    public async ValueTask<IHasPageAttributes?> GetParentAsync() =>
        LowLevel.TryGetValue(KnownNames.Parent, out var parentTask) &&
        await parentTask.CA() is PdfDictionary dict
            ? new HasRenderableContentStream(dict)
            : null;
    
    public ValueTask<long> GetDefaultRotationAsync() => 
        LowLevel.GetOrDefaultAsync(KnownNames.Rotate, 0);
}