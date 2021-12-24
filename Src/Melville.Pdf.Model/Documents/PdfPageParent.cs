using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Documents;

public readonly struct PdfPageParent : IHasPageAttributes
{
    public PdfDictionary LowLevel { get; }

    public PdfPageParent(PdfDictionary lowLevel)
    {
        LowLevel = lowLevel;
    }

    public ValueTask<Stream> GetContentBytes() => new(new MemoryStream());
    
    public ValueTask<IHasPageAttributes?> GetParentAsync() =>
        this.ParentFromAttribute();
}