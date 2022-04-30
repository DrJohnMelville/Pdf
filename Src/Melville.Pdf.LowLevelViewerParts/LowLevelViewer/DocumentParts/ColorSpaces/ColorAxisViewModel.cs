
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.ColorSpaces;

public sealed partial class ColorAxisViewModel
{
    [AutoNotify] private double value = 0.0;
    [AutoNotify] private DeviceColor minColor;
    [AutoNotify] private DeviceColor maxColor;
    public ClosedInterval Interval { get; }

    public ColorAxisViewModel(ClosedInterval interval)
    {
        Interval = interval;
    }
}