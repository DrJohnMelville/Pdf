using Melville.INPC;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.ViewFrames;
using System.IO;
using System.Windows;
using Melville.Fonts;
using Melville.MVVM.Wpf.RootWindows;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ParserMapping;
using Melville.Pdf.LowLevelViewerParts.FontViewers;

namespace Melville.FontViewer.Home;

[OnDisplayed(nameof(LoadFile))]
public class HomeViewModel
{
    public async void LoadFile([FromServices] IOpenSaveFile osf, INavigationWindow window)
    {
        var file = osf.GetLoadFile(null, "ttf", 
            "All Font Files (ttf;ttc;otf;otc;fon;pfa)|*.ttf;*.ttc;*.otf;*.otc;*.fon;*.pfa|" +
            "True Type Font Files (*.ttf)|*.ttf|" +
            "True Type Collections (*.ttc)|*.ttc|" +
            "Open Type Font Files (*.otf)|*.otf|" +
            "Open Type Collections (*.otc)|*.otc|" +
            "Type 1 ASCII Font (*.pfa)|*.pfa|" +
            "Font files (*.fon)|*.fon", "Select File to View");
        if (file == null)
        {
            Application.Current.Shutdown();
            return;
        }

        var map = ParseMap.CreateNew();
        using (var src = await file.OpenRead())
        {
            await map.SetDataAsync(src);
        }

        var multiplexSource = MultiplexSourceFactory.Create(await file.OpenRead());
        map.AddAlias(multiplexSource);
        var fonts = await RootFontParser.ParseAsync(multiplexSource);   
        map.UnRegister();
        window.NavigateTo(new MultiFontViewModel(fonts, map));
    }

}