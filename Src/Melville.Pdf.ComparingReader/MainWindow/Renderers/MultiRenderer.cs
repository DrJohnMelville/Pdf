using System.Collections.Generic;
using Melville.Pdf.LowLevel.Filters.StreamFilters;

namespace Melville.Pdf.ComparingReader.MainWindow.Renderers;

public interface IMultiRenderer
{
    object RenderTarget { get; }
    void SetTarget(MultiBufferStream pdfBits);
}

public abstract class MultiRenderer : IMultiRenderer
{
    public IReadOnlyList<IRenderer> Renderers { get; }

    protected MultiRenderer(IReadOnlyList<IRenderer> renderers)
    {
        this.Renderers = renderers;
    }

    public object RenderTarget => this;

    public void SetTarget(MultiBufferStream pdfBits)
    {
        foreach (var renderer in Renderers)
        {
            renderer.SetTarget(pdfBits.CreateReader());
        }
    }
}

public class TabMultiRendererViewModel : MultiRenderer
{
    public TabMultiRendererViewModel(IList<IRenderer> renderers) : base((IReadOnlyList<IRenderer>)renderers)
    {
    }
}