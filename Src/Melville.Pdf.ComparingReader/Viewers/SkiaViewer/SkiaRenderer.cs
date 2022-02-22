using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Parsing.Streams;
using Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;
using Melville.Pdf.Model.DocumentRenderers;
using Melville.Pdf.SkiaSharp;

namespace Melville.Pdf.ComparingReader.Viewers.SkiaViewer;

public class SkiaRenderer: MelvillePdfRenderer
{
    protected override async ValueTask<ImageSource> Render(DocumentRenderer source, int page)
    {
        var buffer = new MultiBufferStream();
        await RenderWithSkia.ToPngStream(source, page, buffer, -1, 1024);
        return BitmapFrame.Create(
            buffer.CreateReader(), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
    }
}