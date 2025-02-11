using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.ComparingReader.Viewers.ContentStreamOperationsFilters;
using Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;
using Melville.Pdf.FontLibrary;
using Melville.Pdf.FontLibrary.Cjk;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.SkiaSharp;

namespace Melville.Pdf.ComparingReader.Viewers.SkiaViewer;

public class SkiaRenderer: MelvillePdfRenderer
{
    protected override async ValueTask<ImageSource> RenderAsync(DocumentRenderer source, int page)
    {
        using var buffer = WritableBuffer.Create();
        await using var writer = buffer.WritingStream();
        await RenderWithSkia.ToPngStreamAsync(
            source.WithOutputWrapper(ContentStreamOperationFilter.Wrap), page, writer, -1, 4096);
        return BitmapFrame.Create(buffer.ReadFrom(0), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
    }

    protected override IDefaultFontMapper FontSource() => SelfContainedCjkFonts.Instance;
}