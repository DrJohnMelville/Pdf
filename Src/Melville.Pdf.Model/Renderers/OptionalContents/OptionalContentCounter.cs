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
    ValueTask EnterGroupAsync(PdfName oc, PdfName off, IHasPageAttributes attributeSource);
    ValueTask EnterGroupAsync(PdfName oc, PdfDictionary? off);
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

    public async ValueTask EnterGroupAsync(PdfName oc, PdfName off, IHasPageAttributes attributeSource) =>
        await EnterGroupAsync(oc, 
            (await attributeSource.GetResourceAsync(ResourceTypeName.Properties, off).CA()) as PdfDictionary).CA();

    public async ValueTask EnterGroupAsync(PdfName oc, PdfDictionary? off)
    {
        if (IsHidden || (oc==KnownNames.OC && !(await contentState.IsGroupVisibleAsync(off).CA()))) 
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