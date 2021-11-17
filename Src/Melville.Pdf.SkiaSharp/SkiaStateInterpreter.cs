using System.Numerics;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

public static class SkiaStateInterpreter
{
    public static SKPaint Pen(this GraphicsState state)
    {
        var paint = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = (float)state.LineWidth,
            StrokeCap = StrokeCap(state.LineCap),
        };
        SetDashState(state, paint);
        return paint;
    }

    private static void SetDashState(GraphicsState state, SKPaint paint)
    {
        if (state.IsDashedStroke()) 
            paint.PathEffect = CreatePathEffect(state.DashArray, state.DashPhase);
    }

    private static SKPathEffect CreatePathEffect(double[] dashes, double phase) => 
        SKPathEffect.CreateDash(dashes.Select(i => (float)i).ToArray(), (float)phase);

    // by coincidence PDF and skia define these with the same values so we
    // can just cast the enum right across
    private static SKStrokeCap StrokeCap(LineCap cap) => (SKStrokeCap)cap;
    
    public static SKMatrix Transform(this GraphicsState gs) =>
        Transform(gs.TransformMatrix);

    private static SKMatrix Transform(Matrix3x2 tm) => new(
        tm.M11, tm.M21, tm.M31, tm.M12, tm.M22, tm.M32, 0, 0, 1);
}