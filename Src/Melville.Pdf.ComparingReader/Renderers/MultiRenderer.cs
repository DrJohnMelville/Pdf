using System.Collections.Generic;
using System.IO;
using Melville.Pdf.LowLevel.Filters.StreamFilters;

namespace Melville.Pdf.ComparingReader.Renderers;
public interface IRenderer
{
    string DisplayName { get; }
    object RenderTarget { get; }
    void SetTarget(Stream pdfBits);
}

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