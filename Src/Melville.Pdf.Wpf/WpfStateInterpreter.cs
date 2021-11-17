using System.Numerics;
using System.Windows.Media;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf;

public static class WpfStateInterpreter
{
    public static Pen Pen(this GraphicsState state)
    {
        var lineCap = ConvertLineCap(state.LineCap);
        return new Pen(Brushes.Black, state.LineWidth)
        {
            EndLineCap = lineCap,
            StartLineCap = lineCap
        };
    }

    private static PenLineCap ConvertLineCap(LineCap stateLineCap) => stateLineCap switch
      {
          LineCap.Butt => PenLineCap.Flat,
          LineCap.Round => PenLineCap.Round,
          LineCap.Square => PenLineCap.Square,
          _ => throw new ArgumentOutOfRangeException(nameof(stateLineCap), stateLineCap, null)
      };
   
    public static MatrixTransform Transform(this GraphicsState state) => Transform(state.TransformMatrix);
    private static MatrixTransform Transform(Matrix3x2 m) => new(m.M11, m.M12, m.M21, m.M22, m.M31, m.M32);
}