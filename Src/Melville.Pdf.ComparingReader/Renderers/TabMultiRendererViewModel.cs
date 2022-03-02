using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.ComparingReader.Renderers;

public class TabMultiRendererViewModel : MultiRenderer
{
    public IRenderer First => Renderers[0];
    public IRenderer Second => Renderers[1];
    public IRenderer Third => Renderers[2];
    public IEnumerable<IRenderer> Remaining => Renderers.Skip(3);
    public IPageSelector PageSelector { get; }
    public TabMultiRendererViewModel(IList<IRenderer> renderers, IPageSelector pageSelector) : base(
        (IReadOnlyList<IRenderer>)renderers, pageSelector)
    {
        PageSelector = pageSelector;
    }
}