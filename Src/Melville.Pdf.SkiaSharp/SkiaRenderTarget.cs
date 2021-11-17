using System.Numerics;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

public class SkiaRenderTarget:RenderTargetBase<SKCanvas>, IRenderTarget
{
    public SkiaRenderTarget(SKCanvas target, GraphicsStateStack state, PdfPage page) : 
        base(target, state, page)
    {
    }

    public void SetBackgroundRect(PdfRect rect, int width, int height)
    {
        Target.Clear(SKColors.White);
        MapUserSpaceToBitmapSpace(rect, width, height);
    }

    
    #region Path Building

    private SKPath? currentPath = null;
    private SKPath CurrentPath => currentPath ??= new SKPath();

    void IRenderTarget.MoveTo(double x, double y) => CurrentPath.MoveTo((float)x,(float)y);

    void IRenderTarget.LineTo(double x, double y) => CurrentPath.LineTo((float)x, (float)y);

    void IRenderTarget.StrokePath()
    {
        Target.SetMatrix(State.Current().Transform());
        Target.DrawPath(CurrentPath, State.Current().Pen());
        ((IRenderTarget)this).ClearPath();
    }

    void IRenderTarget.ClearPath()
    {
        currentPath = null;
    }

    #endregion
}

public static class SkiaStateInterpreter
{
    public static SKPaint Pen(this GraphicsState state) => new()
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = (float)state.LineWidth
        };

    public static SKMatrix Transform(this GraphicsState gs) =>
        Transform(gs.TransformMatrix);

    private static SKMatrix Transform(Matrix3x2 tm) => new(
        tm.M11, tm.M21, tm.M31, tm.M12, tm.M22, tm.M32, 0, 0, 1);
}