using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage.Streams;
using Melville.INPC;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.ComparingReader.Viewers.WindowsViewer;

public partial class WindowsRendererViewModel : IRenderer
{
    public WindowsRendererViewModel(IPasswordSource passwords)
    {
        this.passwords = passwords;
    }

    public string DisplayName => "Reference Rendering";
    public object RenderTarget => this;
    private PdfDocument? document;
    [AutoNotify] private ImageSource? image;
    private readonly IPasswordSource passwords;

    public async void SetTarget(Stream pdfBits)
    {
        while (true)
        {
            try
            {
                var (password, _) = await passwords.GetPassword();
                pdfBits.Seek(0, SeekOrigin.Begin);
                document = await PdfDocument.LoadFromStreamAsync(
                    pdfBits.AsRandomAccessStream(), password??"");
                var stream = new InMemoryRandomAccessStream();
                await document.GetPage(0).RenderToStreamAsync(stream);
                Image = BitmapFrame.Create(
                    stream.AsStreamForRead(), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
                return;
            }
            catch (COMException)
            {
                    Image = null;
                    return;
            }
        }
    }
}