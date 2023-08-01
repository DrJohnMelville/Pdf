using System.Windows;
using System.Windows.Media;
using Melville.INPC;
using Melville.MVVM.Wpf.ViewFrames;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Wpf;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;

public partial class ImageDisplayViewModel
{
    
    public ImageSource Image { get; init; }
    [AutoNotify] private bool showCheckers = true;
    public double YFactor { get; }
    public ImageDisplayViewModel(ImageSource Image, double yFactor = -1)
    {
        this.Image = Image;
        YFactor = yFactor;
    }

    public void ToggleBackground() => ShowCheckers = !ShowCheckers;

}

public class ImagePartViewModel: StreamPartViewModel
{
    public ImagePartViewModel(string title, IReadOnlyList<DocumentPart> children, PdfValueStream source) : 
        base(title, children, source)
    {
    }

    protected override async ValueTask AddFormatsAsync(List<StreamDisplayFormat> fmts)
    {
        await base.AddFormatsAsync(fmts);
        fmts.Add(new StreamDisplayFormat("Image", async p=>new ImageDisplayViewModel(
            await (await p.WrapForRenderingAsync(new DeviceColor(255,255,255, 255))).ToWpfBitmapAsync())));
    }
}