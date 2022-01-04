using System.Numerics;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

public static class SkiaStateInterpreter
{
    public static SKPaint Brush(this GraphicsState<SKTypeface> state) => 
        new()
        {
            Style = SKPaintStyle.Fill,
            Color = state.NonstrokeColor.AsSkColor(),
        };

    public static SKPaint Pen(this GraphicsState<SKTypeface> state)
    {
        var paint = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            Color = state.StrokeColor.AsSkColor(),
            StrokeWidth = (float)state.EffectiveLineWidth(),
            StrokeCap = StrokeCap(state.LineCap),
            StrokeJoin = CreateStrokeJoin(state.LineJoinStyle),
            StrokeMiter = (float)state.MiterLimit
        };
        SetDashState(state, paint);
        return paint;
    }

    public static SKColor AsSkColor(in this DeviceColor dc) => 
        new(dc.RedByte, dc.GreenByte, dc.BlueByte, dc.Alpha);

    //By coincidence these two enums are equivilent, so a simple cast works.
    private static SKStrokeJoin CreateStrokeJoin(LineJoinStyle joinStyle) => (SKStrokeJoin)joinStyle;

    private static void SetDashState(GraphicsState<SKTypeface> state, SKPaint paint)
    {
        if (state.IsDashedStroke()) 
            paint.PathEffect = CreatePathEffect(state.DashArray, state.DashPhase);
    }

    private static SKPathEffect CreatePathEffect(double[] dashes, double phase) => 
        SKPathEffect.CreateDash(dashes.Select(i => (float)i).ToArray(), (float)phase);

    // by coincidence PDF and skia define these with the same values so we
    // can just cast the enum right across
    private static SKStrokeCap StrokeCap(LineCap cap) => (SKStrokeCap)cap;
    
    public static SKMatrix Transform(this GraphicsState<SKTypeface> gs) =>
        Transform(gs.TransformMatrix);

    public static SKMatrix Transform(this Matrix3x2 tm) => new(
        tm.M11, tm.M21, tm.M31, tm.M12, tm.M22, tm.M32, 0, 0, 1);
}