using System.Windows.Controls;
using System.Windows.Threading;
using Melville.INPC;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model;

namespace Melville.Pdf.Wpf.Controls;

[GenerateDP(typeof(IPasswordSource), "PasswordSource", Nullable = true)]
public partial class PdfViewer : UserControl
{
    /// <summary>
    /// Default Constructor
    /// </summary>
    public PdfViewer()
    {
        InitializeComponent();
    }

    [GenerateDP]
#pragma warning disable Arch004
    private async void OnSourceChanged(object? newSource)
#pragma warning restore Arch004
    {
        if (newSource is null) return;
        var dr = await new PdfReader(PasswordSource).ReadFromAsync(newSource);
        await Dispatcher.BeginInvoke(()=> DataContext = new PdfViewerModel(dr), DispatcherPriority.Normal);
    }
}