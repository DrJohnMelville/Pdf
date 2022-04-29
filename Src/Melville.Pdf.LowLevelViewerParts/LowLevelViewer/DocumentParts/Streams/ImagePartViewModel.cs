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
    public ImageDisplayViewModel(ImageSource Image)
    {
        this.Image = Image;
    }

    public void ToggleBackground() => ShowCheckers = !ShowCheckers;

}

public class ImagePartViewModel: StreamPartViewModel, ICreateView
{
    public ImagePartViewModel(string title, IReadOnlyList<DocumentPart> children, PdfStream source) : 
        base(title, children, source)
    {
    }

    protected override async ValueTask AddFormats(List<StreamDisplayFormat> fmts)
    {
        await base.AddFormats(fmts);
        fmts.Add(new StreamDisplayFormat("Image", async p=>new ImageDisplayViewModel(
            await (await p.WrapForRenderingAsync(null!, new DeviceColor(255,255,255, 255))).ToWbfBitmap())));
    }

    public UIElement View()
    {
        return new StreamPartView{ DataContext = this};
    }
}