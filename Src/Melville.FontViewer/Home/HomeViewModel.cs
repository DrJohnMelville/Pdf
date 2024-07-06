using Melville.INPC;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.ViewFrames;
using System.IO;
using System.Windows;
using Melville.Fonts;
using Melville.MVVM.Wpf.RootWindows;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.LowLevelViewerParts.FontViewers;

namespace Melville.FontViewer.Home;

[OnDisplayed(nameof(LoadFile))]
public class HomeViewModel
{
    public async void LoadFile([FromServices] IOpenSaveFile osf, INavigationWindow window)
    {
        var file = osf.GetLoadFile(null, "ttf", 
            "All Font Files (ttf;ttc;otf;otc;fon)|*.ttf;*.ttc;*.otf;*.otc;*.fon|" +
            "True Type Font Files (*.ttf)|*.ttf|" +
            "True Type Collections (*.ttc)|*.ttc|" +
            "Open Type Font Files (*.otf)|*.otf|" +
            "Open Type Collections (*.otc)|*.otc|" +
            "Font files (*.fon)|*.fon", "Select File to View");
        if (file == null)
        {
            Application.Current.Shutdown();
            return;
        }

        var fonts = await 
            RootFontParser.ParseAsync(
                MultiplexSourceFactory.Create(await file.OpenRead()));
        window.NavigateTo(new MultiFontViewModel(fonts));
    }

}