using Melville.INPC;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.ViewFrames;
using System.IO;
using System.Windows;

namespace Melville.FontViewer.Home;

[OnDisplayed(nameof(LoadFile))]
public partial class HomeViewModel
{
    [AutoNotify] private Stream? data = null;

    public async void LoadFile([FromServices] IOpenSaveFile osf)
    {
        var file = osf.GetLoadFile(null, "ttf", 
            "All Font Files (ttf;ttc;otf;otc;fon)|*.ttf;*.ttc;*.otf;*.otc;*.fon|" +
            "True Type Font Files (*.ttf)|*.ttf|" +
            "True Type Collections (*.ttc)|*.ttc|" +
            "Open Type Font Files (*.otf)|*.otf|" +
            "Open Type Collections (*.otc)|*.otc|" +
            "Font files (*.fon)|*.fon", "Select File to View");
        if (file == null) 
            Application.Current.Shutdown();
        else
            Data = await file.OpenRead();
    }

}