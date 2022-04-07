using System.ComponentModel;
using System.Windows.Media;
using Melville.INPC;
using Melville.Pdf.Model.OptionalContent;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.Wpf.Controls;

public partial class PdfViewerModel
{
    private readonly DocumentRenderer document;
    [AutoNotify] private DrawingVisual? pageImage;
    [AutoNotify] private IReadOnlyList<IOptionalContentDisplayGroup>? optionalContentDisplay;
    public PageSelectorViewModel PageSelector { get; } = new PageSelectorViewModel(); 

    public PdfViewerModel(DocumentRenderer document)
    {
        this.document = document;
        
        InitalizePageFlipper();
        RenderPage(0);
        ConfigureOptionalContentDisplay(this.document.OptionalContentState);
    }

    private void ConfigureOptionalContentDisplay(IOptionalContentState ocs)
    {
        ocs.SelectedContentChanged += RedrawPage;
        if (ocs is INotifyPropertyChanged notifing)
            notifing.WhenMemberChanges(nameof(ocs.SelectedConfiguration), NewConfig);
        NewConfig();
    }
    
    private async void NewConfig()
    {
        OptionalContentDisplay = await document.OptionalContentState.ConstructUiModel(
            document.OptionalContentState.SelectedConfiguration?.Order);
    }

    private void InitalizePageFlipper()
    {
        PageSelector.MaxPage = document.TotalPages;
        PageSelector.PropertyChanged += TryChangePage;
    }

    private void TryChangePage(object? sender, PropertyChangedEventArgs e) => RenderPage(PageSelector.ZeroBasisPage);

    #region Rendering
    private int lastIndex = -1;
    private async void RenderPage(int pageIndex)
    {
        if (pageIndex == lastIndex) return;
        lastIndex = pageIndex;
        var image = await new RenderToDrawingGroup(document, pageIndex).RenderToDrawingVisual();
        PageImage = image;
    }
    private void RedrawPage(object? sender, EventArgs e)
    {
        var savedRenderIndex = lastIndex;
        lastIndex = -1;
        RenderPage(savedRenderIndex);
    }

    #endregion
}