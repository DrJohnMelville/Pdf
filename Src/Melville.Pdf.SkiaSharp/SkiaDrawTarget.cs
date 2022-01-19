using System.Security.AccessControl;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp;

public class SkiaDrawTarget : IDrawTarget, IDisposable
{
    private readonly SKCanvas target;
    private readonly GraphicsStateStack state;
    private SKPath path;

    public SkiaDrawTarget(SKCanvas target, GraphicsStateStack state): this(target, state, new SKPath()){}
    public SkiaDrawTarget(SKCanvas target, GraphicsStateStack state, SKPath path)
    {
        this.target = target;
        this.state = state;
        this.path = path;
    }

    public void Dispose()
    {
        path.Dispose();
    }

    public void MoveTo(double x, double y) => path.MoveTo((float)x,(float)y);

    public void LineTo(double x, double y) => path?.LineTo((float)x, (float)y);

    public void ClosePath()
    {
        path?.Close();
    }

    public void CurveTo(double control1X, double control1Y, double control2X, double control2Y,
        double finalX, double finalY) =>
        path?.CubicTo(
            (float)control1X, (float)control1Y, (float)control2X, (float)control2Y, (float)finalX, (float)finalY);



    public void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        InnerPaintPath(stroke, fill, evenOddFillRule);
    }

    private void InnerPaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        if (fill && state.Current().Brush() is { } brush)
        {
            SetCurrentFillRule(evenOddFillRule);
            target.DrawPath(path, brush);
            brush.Dispose();
        }

        if (stroke && state.Current().Pen() is { } pen)
        {
            target.DrawPath(path, pen);
            pen.Dispose();
        }
    }

    private void SetCurrentFillRule(bool evenOddFillRule) => 
        path.FillType = evenOddFillRule ? SKPathFillType.EvenOdd : SKPathFillType.Winding;


    public void ClipToPath(bool evenOddRule)
    {
        SetCurrentFillRule(evenOddRule);
        target.ClipPath(path);
    }
}