using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Parsing.Streams;
using Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;
using Melville.Pdf.FontLibrary;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.SkiaSharp;

namespace Melville.Pdf.ComparingReader.Viewers.SkiaViewer;

public class SkiaRenderer: MelvillePdfRenderer
{
    protected override async ValueTask<ImageSource> RenderAsync(DocumentRenderer source, int page)
    {
        var buffer = new MultiBufferStream();
        await RenderWithSkia.ToPngStreamAsync(source, page, buffer, -1, 4096);
        return BitmapFrame.Create(
            buffer.CreateReader(), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
    }

    protected override IDefaultFontMapper FontSource() => SelfContainedDefaultFonts.Instance;
}