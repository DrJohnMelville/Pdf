using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Melville.INPC;
using Melville.Parsing.Streams;
using Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;
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
    void SetTarget(MultiBufferStream pdfBits, int showPage = 1);
    Stream GetCurrentTargetReader();
}

public abstract partial class MultiRenderer : IMultiRenderer
{
    [FromConstructor] public IReadOnlyList<IRenderer> Renderers { get; }
    [FromConstructor]private readonly IPageSelector pageSelector;
    [FromConstructor] private readonly IPasswordSource passwordSource;
    private MultiBufferStream? currentTarget;

    partial void OnConstructed() => 
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

    public void SetTarget(MultiBufferStream pdfBits, int showPage)
    {
        currentTarget = pdfBits;
        pageSelector.SetPageSilent(showPage);
        foreach (var renderer in Renderers)
        {
            renderer.SetTarget(pdfBits.CreateReader(), 
                new OneShotPasswordSource(passwordSource));
        }
    }

    public Stream GetCurrentTargetReader() => currentTarget?.CreateReader() as Stream ?? new MemoryStream();
}