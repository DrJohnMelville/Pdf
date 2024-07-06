using System;
using System.Numerics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;

namespace Melville.Pdf.Model.Renderers;

internal sealed partial class PathDrawingAdapter : TrivialPathStateMachine,  IPathDrawingOperations
{
    [FromConstructor] private IDrawTarget? target;
    public bool IsInvalid => target is null;
    private IDrawTarget CurrentShape() => target ?? throw new InvalidOperationException("No current Shape");

    public PathDrawingAdapter WithNewTarget(IDrawTarget target)
    {
        EndPathWithNoOp();
        this.target = target;
        ResetState();
        return this;
    }
    public void EndPathWithNoOp()
    {
        target?.Dispose();
        target = null;
    }

    public void MoveTo(double x, double y)
    {
        CurrentShape().MoveTo(new Vector2((float)x, (float)y));
        RegisterInitialMove(x,y);
    }

    public void LineTo(double x, double y)
    {
        CurrentShape().LineTo(new Vector2((float)x, (float)y));
        RegisterDrawOperationLastPoint(x,y);
    }

    public void CurveTo(double control1X, double control1Y, double control2X, double control2Y, double finalX, double finalY)
    {
        CurrentShape().CurveTo(
            new Vector2((float)control1X, (float)control1Y), 
            new Vector2((float)control2X, (float)control2Y), 
            new Vector2((float)finalX, (float)finalY));
        RegisterDrawOperationLastPoint(finalX, finalY);
    }

    public void CurveToWithoutInitialControl(double control2X, double control2Y, double finalX, double finalY)
    {
        CurrentShape().CurveTo(
            new Vector2((float)CurrentX, (float)CurrentY), 
            new Vector2((float)control2X, (float)control2Y), 
            new Vector2((float)finalX, (float)finalY));
        RegisterDrawOperationLastPoint(finalX, finalY);
    }

    public void CurveToWithoutFinalControl(double control1X, double control1Y, double finalX, double finalY)
    {
        CurrentShape().CurveTo(
            new Vector2((float)control1X, (float)control1Y), 
            new Vector2((float)finalX, (float)finalY), 
            new Vector2((float)finalX, (float)finalY));
        RegisterDrawOperationLastPoint(finalX, finalY);
    }

    public void ClosePath()
    {
        if (IsStartOrInitialMoveState())
            LineTo(FirstX, FirstY);
        CurrentShape().ClosePath();
        RegisterDrawOperationLastPoint(FirstX, FirstY);
    }

    public void Rectangle(double x, double y, double width, double height)
    {
        MoveTo(x,y);
        LineTo(x+width,y);
        LineTo(x+width,y+height);
        LineTo(x,y+height);
        ClosePath();
    }
    
    public void StrokePath() => PaintPath(true, false, false);
    public void CloseAndStrokePath() => CloseAndPaintPath(true, false, false);
    public void FillPath() => PaintPath(false, true, false);
    public void FillPathEvenOdd() => PaintPath(false, true, true);
    public void FillAndStrokePath() => PaintPath(true, true, false);
    public void FillAndStrokePathEvenOdd() => PaintPath(true, true, true);
    public void CloseFillAndStrokePath() => CloseAndPaintPath(true, true, false);
    public void CloseFillAndStrokePathEvenOdd() => CloseAndPaintPath(true, true, true);

    
    private void CloseAndPaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        ClosePath();
        PaintPath(stroke, fill, evenOddFillRule);
    }
    private void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        if (ShouldPaintPath())
            CurrentShape().PaintPath(stroke, fill, evenOddFillRule);
        EndPathWithNoOp();
    }

    public void ClipToPath() => CurrentShape().ClipToPath(false);
    public void ClipToPathEvenOdd() => CurrentShape().ClipToPath(true);
}