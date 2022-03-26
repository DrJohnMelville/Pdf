using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.OptionalContent;

namespace Melville.Pdf.Model.Renderers;

public class OptionalContentCounter
{
    private readonly IOptionalContentState contentState;
    private readonly IHasPageAttributes attributes;
    private uint groupsBelowDeepestVisibleGroup = 0;

    public OptionalContentCounter(IOptionalContentState contentState, IHasPageAttributes attributes)
    {
        this.contentState = contentState;
        this.attributes = attributes;
    }

    public bool IsHidden => groupsBelowDeepestVisibleGroup > 0;

    public async ValueTask EnterGroup(PdfName oc, PdfObject? off)
    {
        if (IsHidden || (oc==KnownNames.OC && !(await contentState.IsGroupVisible(await NameToDict(off).CA()).CA()))) 
            groupsBelowDeepestVisibleGroup++;
    }

    private async ValueTask<PdfDictionary?> NameToDict(PdfObject? obj) => obj switch
    {
        PdfDictionary dict => dict,
        PdfName name => (await attributes.GetResourceAsync(ResourceTypeName.Properties, name).CA())
            as PdfDictionary,
        _ => null,
    };

    public void Pop()
    {
        if (IsHidden) groupsBelowDeepestVisibleGroup--;
    }
}