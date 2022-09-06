using System.Windows.Controls;
using System.Windows.Threading;
using Melville.INPC;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model;

namespace Melville.Pdf.Wpf.Controls;

[GenerateDP(typeof(IPasswordSource), "PasswordSource", Nullable = true)]
public partial class PdfViewer : UserControl
{
    public PdfViewer()
    {
        InitializeComponent();
    }

    [GenerateDP]
    private async void OnSourceChanged(object? newSource)
    {
        if (newSource is null) return;
        var dr = await new PdfReader(PasswordSource).ReadFrom(newSource);
        await Dispatcher.BeginInvoke(()=> DataContext = new PdfViewerModel(dr), DispatcherPriority.Normal);
    }
}