using System.Numerics;
using System.Windows.Media;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf;

public static class WpfStateInterpreter
{
    public static Pen Pen(this GraphicsState state) => new Pen(Brushes.Black, state.LineWidth);

    public static MatrixTransform Transform(this GraphicsState state) => Transform(state.TransformMatrix);
    private static MatrixTransform Transform(Matrix3x2 m) => new(m.M11, m.M12, m.M21, m.M22, m.M31, m.M32);
}