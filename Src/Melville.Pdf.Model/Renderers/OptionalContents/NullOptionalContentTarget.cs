using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.Model.Renderers.OptionalContents;

[StaticSingleton]
internal sealed partial class NullOptionalContentCounter : IOptionalContentCounter
{
    public ValueTask<bool> CanSkipXObjectDoOperationAsync(PdfValueDictionary? visibilityGroup) => new(false);

    public ValueTask EnterGroupAsync(PdfDirectValue oc, PdfDirectValue off, IHasPageAttributes attributeSource) =>
        ValueTask.CompletedTask;
    public ValueTask EnterGroupAsync(PdfDirectValue oc, PdfValueDictionary off) => ValueTask.CompletedTask;
    public void PopContentGroup()
    {
    }
    public IDrawTarget WrapDrawTarget(IDrawTarget inner) => inner;
}