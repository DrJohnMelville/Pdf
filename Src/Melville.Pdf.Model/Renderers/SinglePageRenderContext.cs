using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.ColorOperations;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.OptionalContents;

namespace Melville.Pdf.Model.Renderers;

internal partial class SinglePageRenderContext
{
    [FromConstructor] public IRenderTarget Target { get; }
    [FromConstructor] public DocumentRenderer Renderer { get; }
    [FromConstructor] public IOptionalContentCounter OptionalContent { get;  }
    public PendingItemsStack<PdfObject> ItemsBeingRendered = new();

    public SwitchingColorStrategy CreateColorSwitcher(IHasPageAttributes page) =>
        new(
        Renderer.AdjustColorOperationsModel(new ColorMacroExpansions(Target.GraphicsState, page, Renderer)));
}

public readonly struct PendingItemsStack<T>
{
    private readonly Stack<T> pedingObject = new();

    public PendingItemsStack()
    {
    }

    public bool TryPush(T item)
    {
        if (pedingObject.Contains(item)) return false;
        pedingObject.Push(item);
        return true;
    }

    public void PopItem() => pedingObject.Pop();

}