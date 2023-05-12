using System.Collections.Generic;
using Melville.INPC;

namespace Melville.Pdf.ComparingReader.Renderers
{
    public partial class RenderTab
    {
        public IReadOnlyList<IRenderer> Renderers { get; }
        [AutoNotify] private int selected;

        public RenderTab(IReadOnlyList<IRenderer> renderers, int pos)
        {
            Renderers = renderers;
            selected = pos % renderers.Count;
        }
    }
}