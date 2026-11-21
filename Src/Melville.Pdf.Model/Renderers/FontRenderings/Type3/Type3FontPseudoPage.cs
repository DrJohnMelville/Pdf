using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.FontRenderings.Type3;

internal partial class Type3FontPseudoPage : IHasPageAttributes
{
    [FromConstructor] private readonly IHasPageAttributes parent;
    [FromConstructor] private readonly PdfDictionary fontDecl;
    [FromConstructor] private readonly IPdfStreamReader characterDecl;

    public PdfDictionary LowLevel => fontDecl;
    public ValueTask<IPdfStreamReader> GetContentBytesAsync() => new(characterDecl);

    public ValueTask<IHasPageAttributes?> GetParentAsync() => new(parent);
}