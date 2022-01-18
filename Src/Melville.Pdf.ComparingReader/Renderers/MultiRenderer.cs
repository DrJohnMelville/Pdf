using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Melville.INPC;
using Melville.Parsing.Streams;
using Melville.Pdf.ComparingReader.Renderers.PageFlippers;
using Melville.Pdf.LowLevel.Filters.StreamFilters;

namespace Melville.Pdf.ComparingReader.Renderers;
public interface IRenderer
{
    string DisplayName { get; }
    object RenderTarget { get; }
    void SetTarget(Stream pdfBits, int page);
}

public interface IMultiRenderer
{
    object RenderTarget { get; }
    void SetTarget(MultiBufferStream pdfBits);
}

public abstract class MultiRenderer : IMultiRenderer
{
    private readonly IPageSelector pageSelector;
    public IReadOnlyList<IRenderer> Renderers { get; }
    private MultiBufferStream? source;
    
    protected MultiRenderer(IReadOnlyList<IRenderer> renderers, IPageSelector pageSelector)
    {
        this.pageSelector = pageSelector;
        this.Renderers = renderers;
        (pageSelector as INotifyPropertyChanged)?.WhenMemberChanges(nameof(pageSelector.Page), NewPage);
    }

    private void NewPage()
    {
        if (source is not null) SetTarget(source);
    }

    public object RenderTarget => this;

    public void SetTarget(MultiBufferStream pdfBits)
    {
        if (source != pdfBits)
        {
            pageSelector.Page = 1;
            source = pdfBits;
        }
        foreach (var renderer in Renderers)
        {
            renderer.SetTarget(pdfBits.CreateReader(), pageSelector.Page);
        }
    }
}