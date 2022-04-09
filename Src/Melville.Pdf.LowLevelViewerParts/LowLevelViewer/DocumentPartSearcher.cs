using Melville.MVVM.WaitingServices;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer;

public readonly struct DocumentPartSearcher
{
    private readonly CrossReference target;
    private readonly IWaitingService waiting;
    public DocumentPartSearcher(CrossReference target, IWaitingService waiting)
    {
        this.target = target;
        this.waiting = waiting;
    }

    public async ValueTask<DocumentPart?> FindAsync(IReadOnlyList<DocumentPart> items)
    {
        DocumentPart? ret = null;
        foreach (var item in items)
        {
            if (item.IsTargetOf == target)
            {
                ret = item;
            }
            item.Expanded = false;
            if (await SearchChildren(item) is { } found)
            {
                if (!item.Expanded) item.Expanded = true;
                return found;
            }
        }

        if (ret is not null)
        {
            ret.Expanded = true;
            ret.Selected = true;
        }
        return ret;
    }

    private async ValueTask<DocumentPart?> SearchChildren(DocumentPart item)
    {
        if (item.CanSkipSearch(target.Object)) return null;
        await item.TryFillTree(waiting);
        return await FindAsync(item.Children);
    }
}