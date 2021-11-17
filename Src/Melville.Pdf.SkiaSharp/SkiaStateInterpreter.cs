using System.Numerics;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

public static class SkiaStateInterpreter
{
    public static SKPaint Pen(this GraphicsState state) => new()
    {
        Style = SKPaintStyle.Stroke,
        Color = SKColors.Black,
        StrokeWidth = (float)state.LineWidth,
        StrokeCap = StrokeCap(state.LineCap)
    };

    // by coincidence PDF and skia define these with the same values so we
    // can just cast the enum right across
    private static SKStrokeCap StrokeCap(LineCap cap) => (SKStrokeCap)cap;
    
    public static SKMatrix Transform(this GraphicsState gs) =>
        Transform(gs.TransformMatrix);

    private static SKMatrix Transform(Matrix3x2 tm) => new(
        tm.M11, tm.M21, tm.M31, tm.M12, tm.M22, tm.M32, 0, 0, 1);
}