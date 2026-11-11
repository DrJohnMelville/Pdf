using Melville.INPC;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Wpf.Controls;
using Melville.StreamInterfaces.Adapters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Melville.Pdf.ComparingReader.Viewers.GenericImageViewers;

public interface IImageRenderer
{
    ValueTask SetSourceAsync(Stream pdfBits, IPasswordSource source);
    ValueTask<ImageSource> LoadPageAsync(int page);
    IOptionalContentState? OptionalContentState { get; }
}

[AutoNotify]
public abstract partial class MelvillePdfRenderer : IImageRenderer
{
    private DocumentRenderer? source;

    public IOptionalContentState? OptionalContentState => source?.OptionalContentState;

    public async ValueTask SetSourceAsync(Stream pdfBits, IPasswordSource passwordSource)
    {
        source = null;
        // if the reader throws I want the CodeSource to be null
        source = await new PdfReader(passwordSource, FontSource())
            .ReadFromAsync(pdfBits);
    }

    protected abstract IDefaultFontMapper FontSource();

    public async ValueTask<ImageSource> LoadPageAsync(int page)
    {
        var ret = await TryRenderPageAsync(page);
        ret.Freeze();
        return ret;
    }

    private ValueTask<ImageSource> TryRenderPageAsync(int page) =>
        IsValidPageRender(page)
            ?  RenderAsync(source, page)
            : new(new DrawingImage());

    [MemberNotNullWhen(true, nameof(source))]
    private bool IsValidPageRender(int page) => 
        source != null && page >= 1 && page <= source.TotalPages;

    protected abstract ValueTask<ImageSource> RenderAsync(DocumentRenderer source, int page);
}

public partial class ImageViewerViewModel : IRenderer
{
    public string DisplayName { get; }
    public object RenderTarget => this;
    [AutoNotify] private ImageSource? image;
    [AutoNotify] private string? exception;
    [AutoNotify] public IReadOnlyList<IOptionalContentDisplayGroup>? OptionalContent { get; set; }
    private readonly IImageRenderer renderer;
    private readonly IPageSelector pageSelector;
    private bool fileParseSucceeded = false;

    public ImageViewerViewModel(IImageRenderer renderer, string displayName, IPageSelector pageSelector)
    {
        this.renderer = renderer;
        DisplayName = displayName;
        this.pageSelector = pageSelector;
    }


    public async void SetTarget(Stream pdfBits, IPasswordSource passwordSource)
    {
        fileParseSucceeded = await LoadStreamAsync(pdfBits, passwordSource);
        SetPage(pageSelector.Page);
    }

    private async ValueTask<bool> LoadStreamAsync(Stream pdfBits, IPasswordSource passwordSource)
    {
        try
        {
            if (renderer.OptionalContentState is { } ocs)
                ocs.SelectedContentChanged -= RerenderOptionalContent;
            OptionalContent = null;
            await renderer.SetSourceAsync(pdfBits, passwordSource);
            await TrySetupOptionalContentAsync();
            return true;
        }
        catch (Exception e)
        {
            ReportException(e);
            return false;
        }
    }

    private async Task TrySetupOptionalContentAsync()
    {
        if (renderer.OptionalContentState is { } ops)
        {
            OptionalContent = await ops.ConstructUiModelAsync(
                 renderer.OptionalContentState.SelectedConfiguration?.Order);
            if (OptionalContent.Count < 1) OptionalContent = null;
            renderer.OptionalContentState.SelectedContentChanged += RerenderOptionalContent;
            ((IExternalNotifyPropertyChanged)this).OnPropertyChanged(nameof(OptionalContent));
        }
    }
    void RerenderOptionalContent(object? sender, EventArgs e)
    {
        SetPage(lastPage);
    }

    int lastPage = 1;
    public async void SetPage(int page)
    {
        if  (!fileParseSucceeded) return;
        lastPage = page;
        try
        {
            Image = await renderer.LoadPageAsync(page);
            Exception = null;
        }
        catch (Exception e)
        {
            ReportException(e);
        }
    }

    private void ReportException(Exception e)
    {
        Image = null;
        Exception = $"{e.Message}\r\n\r\n{e.StackTrace}";
    }
}
