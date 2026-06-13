using System.Numerics;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp;

internal class SkiaDrawTarget : IDrawTarget, IDisposable
{
    private readonly SKCanvas target;
    private readonly GraphicsStateStack<SkiaGraphicsState> state;
    private SKPathBuilder? path = null;

    public SkiaDrawTarget(
        SKCanvas target, GraphicsStateStack<SkiaGraphicsState> state)
    {
        this.target = target;
        this.state = state;
    }

    public void Dispose() => path?.Dispose();

    public void MoveTo(Vector2 startPoint) => GetOrCreatePath()
        .MoveTo(startPoint.X, startPoint.Y);

    //The Adobe Pdf interpreter ignores drawing operations before the first MoveTo operation.
    //If path == null then we have not yet gotten a moveto command and we just ignore all the drawing operations
    private SKPathBuilder GetOrCreatePath() => path ??= new SKPathBuilder();

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
        var compositePath = GetPath(evenOddFillRule);
        if (compositePath is null) return;

        if (fill && state.StronglyTypedCurrentState().Brush() is { } brush)
        {
            target.DrawPath(compositePath, brush);
        }

        if (stroke && state.StronglyTypedCurrentState().Pen() is { } pen)
        {
            target.DrawPath(compositePath, pen);
        }
    }

    private SKPath? GetPath(bool evenOddFillRule)
    {
        if (path is null) return null;
        var compositePath = path.Detach();
        compositePath.FillType = FillRule(evenOddFillRule);
        return compositePath;
    }

    private SKPathFillType FillRule(bool evenOddFillRule) => 
        evenOddFillRule ? SKPathFillType.EvenOdd : SKPathFillType.Winding;


    public void ClipToPath(bool evenOddRule)
    {
        var compositePath = GetPath(evenOddRule);
        if (compositePath is null) return;
        target.ClipPath(compositePath);
    }
}