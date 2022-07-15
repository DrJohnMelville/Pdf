using System.IO;
using Melville.INPC;
using Melville.MVVM.WaitingServices;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer;

public partial class LowLevelViewModel
{
    [AutoNotify] private ParsedLowLevelDocument? parsedDoc;
    [AutoNotify] public DocumentPart[]? Root => ParsedDoc?.Root;
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
        ParsedDoc = await TryParse(source);
    }

    private async Task<ParsedLowLevelDocument> TryParse(Stream source)
    {
        try
        {
            return await parser.ParseAsync(source, waiter ?? new FakeWaitingService());
        }
        catch (Exception e)
        {
            return new ParsedLowLevelDocument(
                new[] { new DocumentPart($"Exception: {e.Message}") },
                Array.Empty<CrossReference>());
        }
    }

    public async ValueTask JumpToReference(ReferencePartViewModel target, IWaitingService waiting)
    {
        if (Selected is null) return;
        if (targetHistory.Count == 0 || targetHistory.Peek()!= Selected) targetHistory.Push(Selected);
        Selected = (await new DocumentPartSearcher(target.RefersTo, waiting)
            .FindAsync(Root)) ?? Selected;
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