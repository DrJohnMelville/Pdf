using System.Windows.Controls;
using Melville.INPC;
using Melville.MVVM.WaitingServices;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

public partial class DocumentPart
{
    public string Title { get; }
    public CrossReference IsTargetOf { get; private set; }
    [AutoNotify] private bool expanded;
    [AutoNotify] private bool selected;
    [AutoNotify] private IReadOnlyList<DocumentPart> children;
    public virtual object? DetailView => null;

    public virtual bool CanSkipSearch(int objectNumber) => false;

    public DocumentPart(string title):this(title, Array.Empty<DocumentPart>()) { }
    public DocumentPart(string title, IReadOnlyList<DocumentPart> children)
    {
        Title = title;
        this.children = children;
        IsTargetOf = new CrossReference(-1, -1);
    }

    public DocumentPart WithTarget(int obj, int generation)
    {
        IsTargetOf = new CrossReference(obj, generation);
        return this;
    }
    
        //This is used -- it gets called by the UI
    public async void OnExpand( IWaitingService waiting)
    {
        await TryFillTreeAsync(waiting);
    }
    public void OnSelected(TreeViewItem item)
    {
        item.BringIntoView();
    }
    
    public virtual ValueTask TryFillTreeAsync(IWaitingService waiting) =>
        ValueTask.CompletedTask;

}