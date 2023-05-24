using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.OptionalContents;

[StaticSingleton]
internal sealed partial class NullOptionalContentCounter : IOptionalContentCounter
{
    public ValueTask<bool> CanSkipXObjectDoOperationAsync(PdfDictionary? visibilityGroup) => new(false);

    public ValueTask EnterGroupAsync(PdfName oc, PdfName off, IHasPageAttributes attributeSource) =>
        ValueTask.CompletedTask;
    public ValueTask EnterGroupAsync(PdfName oc, PdfDictionary? off) => ValueTask.CompletedTask;
    public void PopContentGroup()
    {
    }
    public IDrawTarget WrapDrawTarget(IDrawTarget inner) => inner;
}