using System.Windows.Media;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf.Rendering;

public class WpfGraphicsState : GraphicsState<Brush>
{
    protected override Brush CreateSolidBrush(DeviceColor color) => new SolidColorBrush(color.AsWpfColor());
}