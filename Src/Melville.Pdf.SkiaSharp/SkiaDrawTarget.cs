using System;
using System.Numerics;
using System.Security.AccessControl;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp;

public class SkiaGraphicsState:GraphicsState<DeviceColor>
{
    
}
public class SkiaDrawTarget : IDrawTarget, IDisposable
{
    private readonly SKCanvas target;
    private readonly GraphicsStateStack<SkiaGraphicsState> state;
    private SKPath compositePath = new();
    private SKPath path;

    public SkiaDrawTarget(SKCanvas target, GraphicsStateStack<SkiaGraphicsState> state): this(target, state, new SKPath()){}
    public SkiaDrawTarget(SKCanvas target, GraphicsStateStack<SkiaGraphicsState> state, SKPath path)
    {
        this.target = target;
        this.state = state;
        this.path = path;
    }

    public void Dispose()
    {
        path.Dispose();
    }

    public Matrix3x2 currentMatrix = Matrix3x2.Identity;
    public void SetDrawingTransform(Matrix3x2 transform)
    {
        TryAddCurrent();
        currentMatrix = transform;
    }

    private void TryAddCurrent()
    {
        if (path.IsEmpty) return;
        var matrix = currentMatrix.Transform();
        compositePath.AddPath(path, ref matrix);
        path = new SKPath();
    }

    public void MoveTo(double x, double y) => path.MoveTo((float)x,(float)y);

    public void LineTo(double x, double y) => path?.LineTo((float)x, (float)y);

    public void ClosePath()
    {
        path?.Close();
    }

    public void ConicCurveTo(double controlX, double controlY, double finalX, double finalY) =>
        path.QuadTo((float)controlX, (float)controlY, (float)finalX, (float)finalY);

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
        if (fill && state.Current().Brush() is { } brush)
        {
            SetCurrentFillRule(evenOddFillRule);
            target.DrawPath(compositePath, brush);
            brush.Dispose();
        }

        if (stroke && state.Current().Pen() is { } pen)
        {
            target.DrawPath(compositePath, pen);
            pen.Dispose();
        }
    }

    private void SetCurrentFillRule(bool evenOddFillRule) => 
        path.FillType = evenOddFillRule ? SKPathFillType.EvenOdd : SKPathFillType.Winding;


    public void ClipToPath(bool evenOddRule)
    {
        TryAddCurrent();
        SetCurrentFillRule(evenOddRule);
        target.ClipPath(compositePath);
    }
}