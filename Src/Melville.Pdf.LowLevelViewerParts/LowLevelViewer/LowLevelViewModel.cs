using System.IO;
using Melville.INPC;
using Melville.MVVM.WaitingServices;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer;

public partial class LowLevelViewModel
{
    [AutoNotify] private DocumentPart[] root = Array.Empty<DocumentPart>();
    [AutoNotify] private DocumentPart? selected;
    private IWaitingService? waiter;
    private readonly IPartParser parser;

    public LowLevelViewModel(IPartParser parser)
    {
        this.parser = parser;
    }

    public void SetVisualTreeRunner(IWaitingService waiter)
    {
        this.waiter = waiter;
    }

    public async void SetStream(Stream source)
    {
        Root = await parser.ParseAsync(source, waiter ?? new FakeWaitingService());
    }

    public async ValueTask JumpToReference(ReferencePartViewModel target, IWaitingService waiting)
    {
        if (targetHistory.Count == 0 || targetHistory.Peek()!= Selected) targetHistory.Push(Selected);
        Selected = (await new DocumentPartSearcher(target.RefersTo, waiting)
            .FindAsync(root)) ?? Selected;
    }

    private readonly Stack<DocumentPart> targetHistory = new();

    public void NavigateBack()
    {
        if (targetHistory.TryPop(out var sel))
        {
            sel.Selected = true;
            Selected = sel;
        }
    }
}