using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Wpf;

namespace Melville.Pdf.ComparingReader.Viewers.WpfViewers;

public class WpfDrawingGroupRenderer : IImageRenderer
{
    public async ValueTask<ImageSource> LoadFirstPage(Stream pdfBits, string password)
    {
        var doc = await PdfDocument.ReadAsync(pdfBits);
        var pages = await doc.PagesAsync();
        var ret = (await pages.CountAsync()) > 0
            ? new DrawingImage(await new RenderToDrawingGroup().Render(await pages.GetPageAsync(0)))
            : new DrawingImage();
        ret.Freeze();
        return ret;
    }
}