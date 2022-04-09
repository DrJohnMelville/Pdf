using System.Collections.Generic;
using System.Linq;
using Melville.Icc.Model.Tags;
using Melville.INPC;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.ComparingReader.Renderers;

public partial class TabMultiRendererViewModel : MultiRenderer
{
    private readonly IList<IRenderer> renderers;

    public CellConfiguration[] Configurations { get; } =
    {
        new(1,1, "Single"),
        new(2,1, "Horizontal"),
        new(2,2, "Vertical"),
        new(4,2, "Quad"),
        new(6,3, "Six")
    };

    [AutoNotify] private CellConfiguration currentConfiguration;

    private void OnCurrentConfigurationChanged(CellConfiguration config)
    {
        Columns = config.Columns;
        Panes = Enumerable.Range(0, config.Cells).Select(i => new RenderTab(renderers, i)).ToArray();
    }
    [AutoNotify] private IReadOnlyList<RenderTab> panes;
    [AutoNotify] private int columns = 2;
    public IPageSelector PageSelector { get; }
    public TabMultiRendererViewModel(IList<IRenderer> renderers, IPageSelector pageSelector) : base(
        (IReadOnlyList<IRenderer>)renderers, pageSelector)
    {
        this.renderers = renderers;
        PageSelector = pageSelector;
        CurrentConfiguration = Configurations[3];
    }
}

public record CellConfiguration(int Cells, int Columns, string Name);

public partial class RenderTab
{
    public IList<IRenderer> Renderers { get; }
    [AutoNotify] private int selected;

    public RenderTab(IList<IRenderer> renderers, int pos)
    {
        Renderers = renderers;
        selected = pos;
    }
}