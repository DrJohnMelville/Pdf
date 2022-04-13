using System.Windows;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;
using WinRT.Interop;

namespace Melville.Pdf.ComparingReader.Viewers.LowLevel;

public interface IReplStreamPicker
{
    CrossReference? GetReference();
}

public partial class LowLevelRenderViewModel: IReplStreamPicker
{
    [AutoNotify] private Visibility buttonVisibility = Visibility.Collapsed;
    public LowLevelRenderViewModel(LowLevelViewModel innerModel)
    {
        InnerModel = innerModel;
        innerModel.WhenMemberChanges(nameof(innerModel.Selected), () =>
            ButtonVisibility = innerModel.Selected?.IsTargetOf is not null
                ? Visibility.Visible
                : Visibility.Collapsed);
    }

    public LowLevelViewModel InnerModel { get; }

    public CrossReference? GetReference() => (InnerModel.Selected)?.IsTargetOf;
}