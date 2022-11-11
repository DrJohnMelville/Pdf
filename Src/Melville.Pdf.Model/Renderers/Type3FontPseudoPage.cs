using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers;

public partial class Type3FontPseudoPage : IHasPageAttributes
{
    [FromConstructor] private readonly IHasPageAttributes parent;
    [FromConstructor] private readonly PdfDictionary fontDecl;
    [FromConstructor] private readonly Stream characterDecl;

    public PdfDictionary LowLevel => fontDecl;
    public ValueTask<Stream> GetContentBytes() => new(characterDecl);

    public ValueTask<IHasPageAttributes?> GetParentAsync() => new(parent);
}