using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Melville.INPC;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.ComparingReader.Renderers;

public partial class TabMultiRendererViewModel : MultiRenderer
{
    public CellConfiguration[] Configurations { get; } =
    {
        new(1,1, "Single"),
        new(2,1, "Horizontal"),
        new(2,2, "Vertical"),
        new(4,2, "Quad"),
        new(6,3, "Six")
    };

    [AutoNotify] private CellConfiguration currentConfiguration;

    [MemberNotNull(nameof(panes))]
    private void OnCurrentConfigurationChanged(CellConfiguration config)
    {
        var renderList = Renderers.ToList();
        Columns = config.Columns;
        panes = Panes = Enumerable.Range(0, config.Cells)
            .Select(i => new RenderTab(renderList, i)).ToArray();
    }
    [AutoNotify] private IReadOnlyList<RenderTab> panes;
    [AutoNotify] private int columns = 2;
    public IPageSelector PageSelector { get; }

    public TabMultiRendererViewModel(
        IList<IRenderer> renderers, 
        IPageSelector pageSelector, 
        IPasswordSource passwordSource) : base(
        (IReadOnlyList<IRenderer>)renderers, pageSelector, passwordSource)
    {
        PageSelector = pageSelector;
        currentConfiguration = Configurations[3];
        OnCurrentConfigurationChanged(CurrentConfiguration);
        ListenForRenderGroupChange();
    }

    private void ListenForRenderGroupChange()
    {
        foreach (var candidateRenderer in CandidateRenderers)
        {
            candidateRenderer.WhenMemberChanges(nameof(candidateRenderer.IsActive),
                ChangeRenderers);
        }
    }

    private void ChangeRenderers() => 
        OnCurrentConfigurationChanged(CurrentConfiguration);


}

public record CellConfiguration(int Cells, int Columns, string Name);