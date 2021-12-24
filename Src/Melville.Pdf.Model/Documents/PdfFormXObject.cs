
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Documents;

public class PdfFormXObject: IHasPageAttributes
{
    private readonly PdfStream lowLevel;
    private readonly IHasPageAttributes parent;

    public PdfFormXObject(PdfStream lowLevel, IHasPageAttributes parent)
    {
        this.lowLevel = lowLevel;
        this.parent = parent;
    }

    public PdfDictionary LowLevel => lowLevel;

    public ValueTask<Stream> GetContentBytes() => lowLevel.StreamContentAsync();

    public ValueTask<IHasPageAttributes?> GetParentAsync() => new(parent);
}