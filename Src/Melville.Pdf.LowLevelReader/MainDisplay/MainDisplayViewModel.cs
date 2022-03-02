using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.MVVM.WaitingServices;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.ViewFrames;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

namespace Melville.Pdf.LowLevelReader.MainDisplay;

public interface ICloseApp
{
    public void Close();
}
[OnDisplayed(nameof(OpenFile))]
public partial class MainDisplayViewModel
{
    public LowLevelViewModel Model { get; }

    public MainDisplayViewModel(LowLevelViewModel model)
    {
        Model = model;
    }

    public async Task OpenFile([FromServices]IOpenSaveFile dlg, 
        [FromServices] ICloseApp closeApp, IWaitingService wait)
    {
        var file = 
            dlg.GetLoadFile(null, "pdf", "Portable Document Format (*.pdf)|*.pdf", "File to open");
        if (file != null)
        {
            Model.SetStream(await file.OpenRead());
        }
        else
        {
            closeApp.Close();
        }
    }
}