using System.ComponentModel;
using System.Windows.Media;
using Melville.INPC;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.Wpf.Controls;

internal partial class PdfViewerModel
{
    private readonly DocumentRenderer document;
    [AutoNotify] private DrawingVisual? pageImage;
    [AutoNotify] private IReadOnlyList<IOptionalContentDisplayGroup>? optionalContentDisplay;
    public IPageSelector PageSelector { get; } = new PageSelectorViewModel(); 

    public PdfViewerModel(DocumentRenderer document)
    {
        this.document = document;
        
        InitalizePageFlipper();
        RenderPageAsync(1);
        ConfigureOptionalContentDisplay(this.document.OptionalContentState);
    }

    private void ConfigureOptionalContentDisplay(IOptionalContentState ocs)
    {
        ocs.SelectedContentChanged += RedrawPage;
        if (ocs is INotifyPropertyChanged notifing)
            notifing.WhenMemberChanges(nameof(ocs.SelectedConfiguration), NewConfigAsync);
        NewConfigAsync();
    }
    
    private async void NewConfigAsync()
    {
        OptionalContentDisplay = await document.OptionalContentState.ConstructUiModelAsync(
            document.OptionalContentState.SelectedConfiguration?.Order);
    }

    private void InitalizePageFlipper()
    {
        PageSelector.MaxPage = document.TotalPages;
        if (PageSelector is INotifyPropertyChanged inpc)
         inpc.PropertyChanged += TryChangePage;
    }

    private void TryChangePage(object? sender, PropertyChangedEventArgs e) => RenderPageAsync(PageSelector.Page);

    #region Rendering
    private int lastPageNumber = -1;
    private async void RenderPageAsync(int oneBasedPageNumber)
    {
        if (oneBasedPageNumber == lastPageNumber) return;
        lastPageNumber = oneBasedPageNumber;
        var image = await new RenderToDrawingGroup(document, oneBasedPageNumber).RenderToDrawingVisualAsync();
        PageImage = image;
    }
    private void RedrawPage(object? sender, EventArgs e)
    {
        var savedRenderIndex = lastPageNumber;
        lastPageNumber = -1;
        RenderPageAsync(savedRenderIndex);
    }

    #endregion
}