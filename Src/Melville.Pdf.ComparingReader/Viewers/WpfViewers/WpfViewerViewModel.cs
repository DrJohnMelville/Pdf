using System.Threading.Tasks;
using System.Windows.Media;
using Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Wpf;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.ComparingReader.Viewers.WpfViewers;

public class WpfDrawingGroupRenderer : MelvillePdfRenderer
{
    protected override async ValueTask<ImageSource> Render(DocumentRenderer source, int page) => 
        await new RenderToDrawingGroup(source, page).RenderToDrawingImage();
} 