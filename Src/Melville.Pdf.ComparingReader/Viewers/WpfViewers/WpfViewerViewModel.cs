using System.Threading.Tasks;
using System.Windows.Media;
using Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Wpf;

namespace Melville.Pdf.ComparingReader.Viewers.WpfViewers;

public class WpfDrawingGroupRenderer : MelvillePdfRenderer
{
    protected override async ValueTask<ImageSource> Render(PdfPage page) => 
        new DrawingImage(await RenderToDrawingGroup.Render(page));
}