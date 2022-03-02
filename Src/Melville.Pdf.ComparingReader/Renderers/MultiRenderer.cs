using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Melville.INPC;
using Melville.Parsing.Streams;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.ComparingReader.Renderers;
public interface IRenderer
{
    string DisplayName { get; }
    object RenderTarget { get; }
    void SetTarget(Stream pdfBits);
    void SetPage( int page);
}

public interface IMultiRenderer
{
    object RenderTarget { get; }
    void SetTarget(MultiBufferStream pdfBits, int showPage = 1);
    Stream GetCurrentTargetReader();
}

public abstract class MultiRenderer : IMultiRenderer
{
    private readonly IPageSelector pageSelector;
    public IReadOnlyList<IRenderer> Renderers { get; }
    private MultiBufferStream? currentTarget;

    protected MultiRenderer(IReadOnlyList<IRenderer> renderers, IPageSelector pageSelector)
    {
        this.pageSelector = pageSelector;
        this.Renderers = renderers;
        (pageSelector as INotifyPropertyChanged)?.WhenMemberChanges(nameof(pageSelector.Page), NewPage);
    }

    private void NewPage()
    {
        foreach (var renderer in Renderers)
        {
            renderer.SetPage(pageSelector.Page);
        }
    }

    public object RenderTarget => this;

    public void SetTarget(MultiBufferStream pdfBits, int showPage)
    {
        currentTarget = pdfBits;
        pageSelector.SetPageSilent(showPage);
        foreach (var renderer in Renderers)
        {
            renderer.SetTarget(pdfBits.CreateReader());
        }
    }

    public Stream GetCurrentTargetReader() => currentTarget?.CreateReader() as Stream ?? new MemoryStream();
}