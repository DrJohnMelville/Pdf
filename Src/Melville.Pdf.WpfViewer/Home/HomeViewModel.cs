using System.IO;
using System.Windows;
using Melville.INPC;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.ViewFrames;

namespace Melville.Pdf.WpfViewer.Home;

[OnDisplayed(nameof(LoadFile))]
public partial class HomeViewModel
{
    [AutoNotify] private Stream? data = null;

    public async void LoadFile([FromServices] IOpenSaveFile osf)
    {
        var file = osf.GetLoadFile(null, "pdf", "Pdf Files (*.pdf)|*.pdf", "Select File to View");
        if (file == null) 
            Application.Current.Shutdown();
        else
            Data = await file.OpenRead();
    }

}