using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.Wpf.Controls;

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
            WindowsDefaultFonts.Instance);
        await Dispatcher.BeginInvoke(()=> DataContext = new PdfViewerModel(drf), DispatcherPriority.Normal);
    }
}