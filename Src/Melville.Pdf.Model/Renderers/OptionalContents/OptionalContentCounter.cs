using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;

namespace Melville.Pdf.Model.Renderers.OptionalContents;
internal interface IOptionalContentCounter 
{
    ValueTask<bool> CanSkipXObjectDoOperationAsync(PdfDictionary? visibilityGroup);
    ValueTask EnterGroupAsync(PdfDirectObject oc, PdfDirectObject off, IHasPageAttributes attributeSource);
    ValueTask EnterGroupAsync(PdfDirectObject oc, PdfDictionary off);
    void PopContentGroup();
    public IDrawTarget WrapDrawTarget(IDrawTarget inner);
}
internal partial class OptionalContentCounter: IOptionalContentCounter
{
    [FromConstructor]private readonly IOptionalContentState contentState;
    
    private uint groupsBelowDeepestVisibleGroup = 0;

    public bool IsHidden => groupsBelowDeepestVisibleGroup > 0;

    public async ValueTask<bool> CanSkipXObjectDoOperationAsync(PdfDictionary? visibilityGroup) =>
        IsHidden || !await contentState.IsGroupVisibleAsync(visibilityGroup).CA();

    public async ValueTask EnterGroupAsync(
        PdfDirectObject oc, PdfDirectObject off, IHasPageAttributes attributeSource) =>
        await EnterGroupAsync(oc, await TryGetDictionary(off, attributeSource).CA()).CA();

    private static async Task<PdfDictionary?> TryGetDictionary(
        PdfDirectObject off, IHasPageAttributes attributeSource)
    {
        var resource = await attributeSource.GetResourceAsync(ResourceTypeName.Properties, off).CA();
        return resource.TryGet(out PdfDictionary? dict) ? dict : null;
    }

    public async ValueTask EnterGroupAsync(PdfDirectObject oc, PdfDictionary? off)
    {
        if (IsHidden || (oc.Equals(KnownNames.OCTName) && !await contentState.IsGroupVisibleAsync(off).CA())) 
            groupsBelowDeepestVisibleGroup++;
    } 
    public void PopContentGroup()
    {
        if (IsHidden) groupsBelowDeepestVisibleGroup--;
    }


    private static readonly ObjectRentalManager<OptionalContentDrawTarget> targets = new (10);
    public IDrawTarget WrapDrawTarget(IDrawTarget inner) => targets.Rent().With(this, inner);
    internal void ReturnDrawTarget(OptionalContentDrawTarget item) => targets.Return(item);
}