using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.SkiaSharp;

namespace Melville.Pdf.ComparingReader.Viewers.SkiaViewer;

public class SkiaRenderer: MelvillePdfRenderer
{
    protected override async ValueTask<ImageSource> Render(PdfPage page)
    {
        var buffer = new MultiBufferStream();
        await RenderWithSkia.ToPngStream(page, buffer);
        return BitmapFrame.Create(
            buffer.CreateReader(), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
    }
}