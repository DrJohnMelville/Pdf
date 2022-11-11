using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;

namespace Melville.Pdf.Model.Renderers.OptionalContents;

public interface IOptionalContentTarget 
{
    ValueTask<bool> CanSkipXObjectDoOperation(PdfDictionary? visibilityGroup);
    ValueTask EnterGroup(PdfName oc, PdfName off, IHasPageAttributes attributeSource);
    ValueTask EnterGroup(PdfName oc, PdfDictionary? off);
    void PopContentGroup();
}

public partial class OptionalContentTarget: IOptionalContentTarget, IRenderTarget
{
    [FromConstructor]private readonly IOptionalContentState contentState;
    [DelegateTo] [FromConstructor] private IRenderTarget innerTarget;
    
    private uint groupsBelowDeepestVisibleGroup = 0;

    public bool IsHidden => groupsBelowDeepestVisibleGroup > 0;

    public async ValueTask<bool> CanSkipXObjectDoOperation(PdfDictionary? visibilityGroup) =>
        IsHidden || !await contentState.IsGroupVisible(visibilityGroup).CA();

    public async ValueTask EnterGroup(PdfName oc, PdfName off, IHasPageAttributes attributeSource) =>
        await EnterGroup(oc, 
            (await attributeSource.GetResourceAsync(ResourceTypeName.Properties, off).CA()) as PdfDictionary).CA();

    public async ValueTask EnterGroup(PdfName oc, PdfDictionary? off)
    {
        if (IsHidden || (oc==KnownNames.OC && !(await contentState.IsGroupVisible(off).CA()))) 
            groupsBelowDeepestVisibleGroup++;
    } 
    public void PopContentGroup()
    {
        if (IsHidden) groupsBelowDeepestVisibleGroup--;
    }
    
    public IDrawTarget CreateDrawTarget() => new OptionalContentDrawTarget(this, innerTarget.CreateDrawTarget());
}