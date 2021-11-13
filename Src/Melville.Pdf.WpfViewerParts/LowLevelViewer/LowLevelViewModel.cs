using System.IO;
using Melville.INPC;
using Melville.MVVM.WaitingServices;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.EventBindings.SearchTree;
using Melville.MVVM.Wpf.ViewFrames;
using Melville.Pdf.WpfViewerParts.LowLevelViewer.DocumentParts;

namespace Melville.Pdf.WpfViewerParts.LowLevelViewer;

public partial class LowLevelViewModel
{
    [AutoNotify] private DocumentPart[] root = Array.Empty<DocumentPart>();
    [AutoNotify] private DocumentPart? selected;
    private IWaitingService? waiter;
    private IPartParser? parser;

    public void SetVisualTreeRunner(IWaitingService waiter, [FromServices] IPartParser parser)
    {
        this.waiter = waiter;
        this.parser = parser;
    }

    public async void SetStream(Stream source)
    {
        if (parser is null || waiter is null) return;
        Root = await parser.ParseAsync(source, waiter);
    }
}