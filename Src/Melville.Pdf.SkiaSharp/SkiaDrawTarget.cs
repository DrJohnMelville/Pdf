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
        //TryAddCurrent();
        //currentMatrix = transform;
    }

    private void TryAddCurrent()
    {
        if (path is null or {IsEmpty:true} ) return;
        var matrix = currentMatrix.Transform();
        compositePath.AddPath(path, ref matrix);
        path = new SKPath();
    }

    public void MoveTo(Vector2 startPoint) => GetOrCreatePath()
        .MoveTo(startPoint.X, startPoint.Y);

    //The Adobe Pdf interpreter ignores drawing operations before the first MoveTo operation.
    //If path == null then we have not yet gotten a moveto command and we just ignore all the drawing operations
    private SKPath GetOrCreatePath() => path ??= new SKPath();

    public void LineTo(Vector2 endPoint) => path?.LineTo(
        endPoint.X, endPoint.Y);

    public void ClosePath()
    {
        path?.Close();
    }

    public void CurveTo(Vector2 control, Vector2 endPoint) =>
        path?.QuadTo(control.X, control.Y, endPoint.X, endPoint.Y);

    public void CurveTo(Vector2 control1, Vector2 control2, Vector2 endPoint) =>
        path?.CubicTo(control1.X, control1.Y, control2.X, control2.Y,
            endPoint.X, endPoint.Y);

    public void EndGlyph()
    {
    }

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