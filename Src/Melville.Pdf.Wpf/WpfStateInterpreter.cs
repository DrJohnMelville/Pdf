using System.Numerics;
using System.Windows.Media;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf;

public static class WpfStateInterpreter
{
    public static Brush Brush(this GraphicsState<GlyphTypeface> state) => state.NonstrokeColor.AsSolidBrush();
    
    public static Pen Pen(this GraphicsState<GlyphTypeface> state)
    {
        var lineCap = ConvertLineCap(state.LineCap);
        var pen = new Pen(Brushes.Black, state.EffectiveLineWidth())
        {
            EndLineCap = lineCap,
            StartLineCap = lineCap,
            DashCap = lineCap,
            DashStyle = ComputeDashStyle(state),
            LineJoin = ComputeLineJoin(state.LineJoinStyle),
            MiterLimit = state.MiterLimit,
            Brush = state.StrokeColor.AsSolidBrush()
        };
        return pen;
    }

    public static Brush AsSolidBrush(in this DeviceColor dc) => new SolidColorBrush(dc.AsWpfColor());

    private static Color AsWpfColor(in this DeviceColor dc) => 
        Color.FromArgb(dc.Alpha, dc.RedByte, dc.GreenByte, dc.BlueByte);

    private static PenLineJoin ComputeLineJoin(LineJoinStyle joinStyle) => joinStyle switch
    {
        LineJoinStyle.Miter => PenLineJoin.Miter,
        LineJoinStyle.Round => PenLineJoin.Round,
        LineJoinStyle.Bevel => PenLineJoin.Bevel,
        _ => throw new ArgumentOutOfRangeException(nameof(joinStyle), joinStyle, null)
    };
    
    private static DashStyle ComputeDashStyle(GraphicsState<GlyphTypeface> state) => 
        state.IsDashedStroke() ?
            CustomDashStyle(state.DashArray, state.DashPhase, state.LineWidth):
            DashStyles.Solid;

    private static DashStyle CustomDashStyle(double[] dashes, double phase, double width) =>
        new(dashes.Select(i => i / width).ToArray(), phase / width);

    private static PenLineCap ConvertLineCap(LineCap stateLineCap) => 
        stateLineCap switch
      {
          LineCap.Butt => PenLineCap.Flat,
          LineCap.Round => PenLineCap.Round,
          LineCap.Square => PenLineCap.Square,
          _ => throw new ArgumentOutOfRangeException(nameof(stateLineCap), stateLineCap, null)
      };
   
    public static MatrixTransform Transform(this GraphicsState<GlyphTypeface> state) => 
        WpfTransform(state.TransformMatrix);
    public static MatrixTransform WpfTransform(this Matrix3x2 m) => new(m.M11, m.M12, m.M21, m.M22, m.M31, m.M32);
}