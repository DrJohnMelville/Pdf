using System.IO;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.INPC;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.EventBindings.SearchTree;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.ViewFrames;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;

namespace Melville.Pdf.LowLevelViewer.MainDisplay;

public interface ICloseApp
{
    public void Close();
}
[OnDisplayed(nameof(OpenFileAsync))]
public partial class MainDisplayViewModel
{
    [AutoNotify] private object? model;

    public MainDisplayViewModel(LowLevelViewModel model)
    {
        Model = model;
    }

    public async Task OpenFileAsync([FromServices]IOpenSaveFile dlg, 
        [FromServices] ICloseApp closeApp, IVisualTreeRunner runner)
    {
        var file = 
            dlg.GetLoadFile(null, "pdf", "Portable Document Format|*.pdf|", "File to open");
        switch (file?.Extension().ToUpper())
        {
            case "PDF":
                runner.RunMethod(OpenPdfFile, new object?[] { await file.OpenRead() }, out var _);
                break;
            default:
                closeApp.Close();
                break;
        }
    }

    private void OpenPdfFile(Stream pdfFile, [FromServices] LowLevelViewModel newModel)
    {
        newModel.SetStream(pdfFile);
        Model = newModel;
    }
}