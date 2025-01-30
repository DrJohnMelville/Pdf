using System.Threading.Tasks;
using System.Windows.Media;
using Melville.Pdf.ComparingReader.Viewers.ContentStreamOperationsFilters;
using Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.ComparingReader.Viewers.WpfViewers;

public class WpfDrawingGroupRenderer : MelvillePdfRenderer
{
    protected override async ValueTask<ImageSource> RenderAsync(DocumentRenderer source, int page) => 
        await new RenderToDrawingGroup(source.WithOutputWrapper(
            ContentStreamOperationFilter.Wrap), page).RenderToDrawingImageAsync();
    protected override IDefaultFontMapper FontSource() => WindowsDefaultFonts.Instance;

} 