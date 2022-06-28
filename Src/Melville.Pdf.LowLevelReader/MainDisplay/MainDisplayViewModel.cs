using System.IO;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.INPC;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.EventBindings.SearchTree;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.ViewFrames;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevelReader.ImageViewers;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.LowLevelReader.MainDisplay;

public interface ICloseApp
{
    public void Close();
}
[OnDisplayed(nameof(OpenFile))]
public partial class MainDisplayViewModel
{
    [AutoNotify] private object? model;

    public MainDisplayViewModel(LowLevelViewModel model)
    {
        Model = model;
    }

    public async Task OpenFile([FromServices]IOpenSaveFile dlg, 
        [FromServices] ICloseApp closeApp, IVisualTreeRunner runner)
    {
        var file = 
            dlg.GetLoadFile(null, "pdf", "Image Files|*.pdf;*.jpg;;*.jp2;*.jpx|Portable Document Format|*.pdf|" +
                                         "Jpeg|*.jpg|Jpeg 2000|*.jp2|JPX|*.jpx", "File to open");
        switch (file?.Extension().ToUpper())
        {
            case "PDF":
                runner.RunMethod(OpenPdfFile, new object?[] { await file.OpenRead() }, out var _);
                break;
            case "JPG":
            case "JP2":
            case "JPX":
                Model = new ImageDisplayViewModel(await ImageReader.ReadJpeg(file), -1);
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