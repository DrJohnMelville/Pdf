using System.Numerics;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp;

internal class SkiaDrawTarget : IDrawTarget, IDisposable
{
    private readonly SKCanvas target;
    private readonly GraphicsStateStack<SkiaGraphicsState> state;
    private SKPath compositePath = new();
    private SKPath? path = null;

    public SkiaDrawTarget(
        SKCanvas target, GraphicsStateStack<SkiaGraphicsState> state)
    {
        this.target = target;
        this.state = state;
    }

    public void Dispose() => path?.Dispose();

    private Matrix3x2 currentMatrix = Matrix3x2.Identity;
    public void SetDrawingTransform(in Matrix3x2 transform)
    {
        TryAddCurrent();
        currentMatrix = transform;
    }

    private void TryAddCurrent()
    {
        if (path is null or {IsEmpty:true} ) return;
        var matrix = currentMatrix.Transform();
        compositePath.AddPath(path, ref matrix);
        path = new SKPath();
    }

    public void MoveTo(double x, double y) => GetOrCreatePath().MoveTo((float)x,(float)y);

    //The Adobe Pdf interpreter ignores drawing operations before the first MoveTo operation.
    //If path == null then we have not yet gotten a moveto command and we just ignore all the drawing operations
    private SKPath GetOrCreatePath() => path ??= new SKPath();

    public void LineTo(double x, double y) => path?.LineTo((float)x, (float)y);

    public void ClosePath()
    {
        path?.Close();
    }

    public void ConicCurveTo(double controlX, double controlY, double finalX, double finalY) =>
        path?.QuadTo((float)controlX, (float)controlY, (float)finalX, (float)finalY);

    public void CurveTo(double control1X, double control1Y, double control2X, double control2Y,
        double finalX, double finalY) =>
        path?.CubicTo(
            (float)control1X, (float)control1Y, (float)control2X, (float)control2Y, (float)finalX, (float)finalY);



    public void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        TryAddCurrent();
        InnerPaintPath(stroke, fill, evenOddFillRule);
    }

    private void InnerPaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        if (fill && state.StronglyTypedCurrentState().Brush() is { } brush)
        {
            SetCurrentFillRule(evenOddFillRule);
            target.DrawPath(compositePath, brush);
        }

        if (stroke && state.StronglyTypedCurrentState().Pen() is { } pen)
        {
            target.DrawPath(compositePath, pen);
        }
    }

    private void SetCurrentFillRule(bool evenOddFillRule) => 
        compositePath.FillType = evenOddFillRule ? SKPathFillType.EvenOdd : SKPathFillType.Winding;


    public void ClipToPath(bool evenOddRule)
    {
        TryAddCurrent();
        SetCurrentFillRule(evenOddRule);
        target.ClipPath(compositePath);
    }
}