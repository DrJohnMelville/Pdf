using Melville.Pdf.LowLevelViewerParts.LowLevelViewer;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;

namespace Melville.Pdf.ComparingReader.Viewers.LowLevel;

public interface IReplStreamPicker
{
    CrossReference? GetReference();
}

public partial class LowLevelRenderViewModel: IReplStreamPicker
{
    public LowLevelRenderViewModel(LowLevelViewModel innerModel)
    {
        InnerModel = innerModel;
    }

    public LowLevelViewModel InnerModel { get; }

    public CrossReference? GetReference() => (InnerModel.Selected)?.IsTargetOf;
}