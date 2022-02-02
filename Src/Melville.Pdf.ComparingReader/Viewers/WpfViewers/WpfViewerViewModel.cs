using System.Threading.Tasks;
using System.Windows.Media;
using Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;
using Melville.Pdf.Model.DocumentRenderers;
using Melville.Pdf.Wpf;

namespace Melville.Pdf.ComparingReader.Viewers.WpfViewers;

public class WpfDrawingGroupRenderer : MelvillePdfRenderer
{
    protected override async ValueTask<ImageSource> Render(DocumentRenderer source, int page) => 
        new DrawingImage(await new RenderToDrawingGroup().Render(source, page));
} 