using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.MVVM.WaitingServices;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.ViewFrames;
using Melville.Pdf.WpfViewerParts.LowLevelViewer.DocumentParts;

namespace Melville.Pdf.LowLevelReader.MainDisplay;

public interface ICloseApp
{
    public void Close();
}
[OnDisplayed(nameof(OpenFile))]
public partial class MainDisplayViewModel
{
    [AutoNotify] private DocumentPart[] root = Array.Empty<DocumentPart>();
    [AutoNotify] private DocumentPart? selected;

    public async Task OpenFile([FromServices]IOpenSaveFile dlg, [FromServices]IPartParser parser, 
        [FromServices] ICloseApp closeApp, IWaitingService wait)
    {
        var file = 
            dlg.GetLoadFile(null, "pdf", "Portable Document Format (*.pdf)|*.pdf", "File to open");
        if (file != null)
        {
            Root = await parser.ParseAsync(file, wait);
        }
        else
        {
            closeApp.Close();
        }
    }
}