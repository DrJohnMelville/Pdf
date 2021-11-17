using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.ComparingReader.Viewers.LowLevel;

namespace Melville.Pdf.ComparingReader.Renderers;

public class TabMultiRendererViewModel : MultiRenderer
{
    public IRenderer First => Renderers[0];
    public IRenderer Second => Renderers[1];
    public IRenderer Third => Renderers[2];
    public IEnumerable<IRenderer> Remaining => Renderers.Skip(3);
    public TabMultiRendererViewModel(IList<IRenderer> renderers) : base((IReadOnlyList<IRenderer>)renderers)
    {
    }
}