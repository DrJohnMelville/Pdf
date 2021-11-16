using System.Windows.Media;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf;

public static class WpfStateInterpreter
{
    public static Pen Pen(this GraphicsState state)
    {
        return new Pen(Brushes.Blue, state.LineWidth);
    }
}