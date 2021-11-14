using System.IO;
using Melville.INPC;
using Melville.MVVM.WaitingServices;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.Pdf.WpfViewerParts.LowLevelViewer.DocumentParts;

namespace Melville.Pdf.WpfViewerParts.LowLevelViewer;

public partial class LowLevelViewModel
{
    [AutoNotify] private DocumentPart[] root = Array.Empty<DocumentPart>();
    [AutoNotify] private DocumentPart? selected;
    private IWaitingService? waiter;
    private readonly IPartParser parser;

    public LowLevelViewModel(IPartParser? parser)
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
}