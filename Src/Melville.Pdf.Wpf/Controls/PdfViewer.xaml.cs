using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.Model.DocumentRenderers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.Wpf.Controls;

public partial class PdfViewerModel
{
    private readonly DocumentRenderer document;
    [AutoNotify] private ImageSource? pageImage;
    public PageSelectorViewModel PageSelector { get; } = new PageSelectorViewModel(); 

    public PdfViewerModel(DocumentRenderer document)
    {
        this.document = document;
        InitalizePageFlipper();
        RenderPage(0);
    }

    private void InitalizePageFlipper()
    {
        PageSelector.MaxPage = document.TotalPages;
        PageSelector.PropertyChanged += TryChangePage;
    }

    private void TryChangePage(object? sender, PropertyChangedEventArgs e) => RenderPage(PageSelector.ZeroBasisPage);

    private int lastIndex = -1;
    private async void RenderPage(int pageIndex)
    {
        if (pageIndex == lastIndex) return;
        lastIndex = pageIndex;
        var image = await new RenderToDrawingGroup(document, pageIndex).RenderToDrawingImage();
        PageImage = image;
    }
}

public partial class PdfViewer : UserControl
{
    public PdfViewer()
    {
        InitializeComponent();
    }

    [GenerateDP]
    private void OnSourceChanged(object? newSource)
    {
        switch (newSource)
        {
            case null: break;
            case string s: SetTo(s);
                break;
            case byte[] buf: SetTo(buf);
                break;
            case Stream s: SetTo(s);
                break;
            case PdfLowLevelDocument lld: SetTo(lld);
                break;
            case PdfDocument doc: SetTo(doc);
                break;
            default: // Silently fail if unrecognized type
                break;
        }
        
    }

    private void SetTo(String fileName) => SetTo(File.OpenRead(fileName));
    private void SetTo(byte[] buf) => SetTo(new MemoryStream(buf));
    private async void SetTo(Stream s) => SetTo(await RandomAccessFileParser.Parse(s));
    private void SetTo(PdfLowLevelDocument document) => SetTo(new PdfDocument(document));
    private async void SetTo(PdfDocument document)
    {
        var drf = await DocumentRendererFactory.CreateRendererAsync(document,
            new WindowsDefaultFonts());
        await Dispatcher.BeginInvoke(()=> DataContext = new PdfViewerModel(drf), DispatcherPriority.Normal);
    }
}