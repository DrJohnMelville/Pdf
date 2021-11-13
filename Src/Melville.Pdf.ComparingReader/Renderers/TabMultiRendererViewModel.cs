using System.Collections.Generic;
using Melville.Pdf.ComparingReader.Viewers.LowLevel;

namespace Melville.Pdf.ComparingReader.Renderers;

public class TabMultiRendererViewModel : MultiRenderer
{
    public TabMultiRendererViewModel(IList<IRenderer> renderers) : base((IReadOnlyList<IRenderer>)renderers)
    {
    }
}