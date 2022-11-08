using System.Linq;
using System.Numerics;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.SkiaSharp;
using SkiaSharp;

public static class SkiaStateInterpreter
{
    public static SKPaint Brush(this SkiaGraphicsState state)
    {
        var ret = state.NonstrokeBrush.CreateBrush(state);
        ret.Style = SKPaintStyle.Fill;
        return ret;
    }

    public static SKPaint Pen(this SkiaGraphicsState state)
    {
        var paint = state.StrokeBrush.CreateBrush(state);
        paint.Style = SKPaintStyle.Stroke;
        paint.Color = state.StrokeColor.AsSkColor();
        paint.StrokeWidth = (float)state.EffectiveLineWidth();
        paint.StrokeCap = StrokeCap(state.LineCap);
        paint.StrokeJoin = CreateStrokeJoin(state.LineJoinStyle);
        paint.StrokeMiter = (float)state.MiterLimit;

        SetDashState(state, paint);
        return paint;
    }

    public static SKColor AsSkColor(in this DeviceColor dc) => 
        new(dc.RedByte, dc.GreenByte, dc.BlueByte, dc.Alpha);

    //By coincidence these two enums are equivilent, so a simple cast works.
    private static SKStrokeJoin CreateStrokeJoin(LineJoinStyle joinStyle) => (SKStrokeJoin)joinStyle;

    private static void SetDashState(GraphicsState state, SKPaint paint)
    {
        if (state.IsDashedStroke()) 
            paint.PathEffect = CreatePathEffect(state.DashArray, state.DashPhase);
        else
        {
            paint.PathEffect = null;
        }
    }

    private static SKPathEffect CreatePathEffect(double[] dashes, double phase) => 
        SKPathEffect.CreateDash(CreateDashArray(dashes), (float)phase);

    private static float[] CreateDashArray(double[] dashes)
    {
        if (dashes.Length % 2 == 0)
            return dashes.Select(i => (float)i).ToArray();
        else
            return dashes.Concat(dashes).Select(i => (float)i).ToArray();
    }

    // by coincidence PDF and skia define these with the same values so we
    // can just cast the enum right across
    private static SKStrokeCap StrokeCap(LineCap cap) => (SKStrokeCap)cap;
    
    public static SKMatrix Transform(this GraphicsState gs) =>
        Transform(gs.TransformMatrix);

    public static SKMatrix Transform(this Matrix3x2 tm) => new(
        tm.M11, tm.M21, tm.M31, tm.M12, tm.M22, tm.M32, 0, 0, 1);
}