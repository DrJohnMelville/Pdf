using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Melville.INPC;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Wpf.Controls;

namespace Melville.Pdf.ComparingReader.Renderers;
public interface IRenderer
{
    string DisplayName { get; }
    object RenderTarget { get; }
    void SetTarget(Stream pdfBits, IPasswordSource passwordSource);
    void SetPage( int page);
}

public interface IMultiRenderer
{
    object RenderTarget { get; }
    void SetTarget(IMultiplexSource pdfBits, int showPage = 1);
    Stream GetCurrentTargetReader();
}

public partial class RendererViewModel
{
    [AutoNotify] private bool isActive = true;
    [FromConstructor] public IRenderer Renderer { get; }
}

public abstract partial class MultiRenderer : IMultiRenderer
{
    public IReadOnlyList<RendererViewModel> CandidateRenderers { get; }
    protected IEnumerable<IRenderer> Renderers => CandidateRenderers
        .Where(i => i.IsActive)
        .Select(i=>i.Renderer);
    private readonly IPageSelector pageSelector;
    private readonly IPasswordSource passwordSource;
    private IMultiplexSource? currentTarget;

    protected MultiRenderer(IReadOnlyList<IRenderer> renderers, IPageSelector pageSelector, IPasswordSource passwordSource)
    {
        CandidateRenderers = renderers.Select(i => new RendererViewModel(i)).ToList();
        this.pageSelector = pageSelector;
        this.passwordSource = passwordSource;
        ListenForPageChange();
    }


    private void ListenForPageChange() => 
        (pageSelector as INotifyPropertyChanged)?
        .WhenMemberChanges(nameof(pageSelector.Page), NewPage);
    
    private void NewPage()
    {
        foreach (var renderer in Renderers)
        {
            renderer.SetPage(pageSelector.Page);
        }
    }

    public object RenderTarget => this;

    public void SetTarget(IMultiplexSource pdfBits, int showPage)
    {
        currentTarget?.Dispose();
        currentTarget = pdfBits;
        pageSelector.SetPageSilent(showPage);
        foreach (var renderer in Renderers)
        {
            renderer.SetTarget(pdfBits.ReadFrom(0), 
                new OneShotPasswordSource(passwordSource));
        }
    }

    public Stream GetCurrentTargetReader() => (currentTarget != null ? ((IMultiplexSource)currentTarget).ReadFrom(0) : null) as Stream ?? new MemoryStream();
}