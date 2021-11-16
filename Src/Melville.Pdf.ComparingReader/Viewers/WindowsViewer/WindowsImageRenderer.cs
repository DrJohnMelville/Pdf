using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage.Streams;
using Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;

namespace Melville.Pdf.ComparingReader.Viewers.WindowsViewer;

public class WindowsImageRenderer : IImageRenderer
{
    public  async ValueTask<ImageSource> LoadFirstPage(Stream pdfBits, string password)
    {
        var document = await PdfDocument.LoadFromStreamAsync(
            pdfBits.AsRandomAccessStream(), password);
        var stream = new InMemoryRandomAccessStream();
        await document.GetPage(0).RenderToStreamAsync(stream);
        var bitmapFrame = BitmapFrame.Create(
            stream.AsStreamForRead(), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
        return bitmapFrame;
    }
}