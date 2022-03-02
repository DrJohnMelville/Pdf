using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage.Streams;
using Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.ComparingReader.Viewers.WindowsViewer;

public class WindowsImageRenderer : IImageRenderer
{
    private readonly IPageSelector pageSel;
    private PdfDocument? document;
    
    public WindowsImageRenderer(IPageSelector pageSel)
    {
        this.pageSel = pageSel;
    }

    public async ValueTask SetSource(Stream pdfBits, string password)
    {
        document = null;
        document = await PdfDocument.LoadFromStreamAsync(pdfBits.AsRandomAccessStream(), password);
        TrySetPageCount((int)document.PageCount);
    }

    public async ValueTask<ImageSource> LoadPage(int page)
    {
        if (document == null) return new BitmapImage();
        
        var stream = new InMemoryRandomAccessStream();
        await document.GetPage((uint)page - 1).RenderToStreamAsync(stream);
        var bitmapFrame = BitmapFrame.Create(
            stream.AsStreamForRead(), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
        return bitmapFrame;
    }

    private void TrySetPageCount(int pageCount)
    {
        if (pageSel.MaxPage != pageCount) pageSel.MaxPage = pageCount;
    }
}