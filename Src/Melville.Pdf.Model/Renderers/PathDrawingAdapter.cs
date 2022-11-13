using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

// Pdf standard 2.0 section 8.5.3.2 gives very detailed rules for the stroking trivial paths

internal enum TrivialPathDetectorState
{
    Start, InitialMoveTo, MustDraw, ShouldNotDraw
}

public sealed partial class PathDrawingAdapter : IPathDrawingOperations
{
    [FromConstructor] private IDrawTarget? target;
    [FromConstructor] private readonly IGraphicsState graphicsState;
    private TrivialPathDetectorState state = TrivialPathDetectorState.Start;
    public bool IsInvalid => target is null;
    private IDrawTarget CurrentShape() => target ?? throw new InvalidOperationException("No current Shape");

    public PathDrawingAdapter WithNewTarget(IDrawTarget target)
    {
        this.target = target;
        firstX = firstY = lastX = lasty = 0.0;
        state = TrivialPathDetectorState.Start;
        return this;
    }
    public void EndPathWithNoOp()
    {
        (target as IDisposable)?.Dispose();
        target = null;
    }
    
    private double firstX, firstY;
    private double lastX, lasty;

    private void SetLast(double x, double y)
    {
        (lastX, lasty) = (x, y);
        CheckLineForStateChange();
    }

    private void SetFirst(double x, double y)
    {
        (lastX, lasty) = (firstX, firstY) = (x, y);
        if (state is TrivialPathDetectorState.Start) state = TrivialPathDetectorState.InitialMoveTo;
    }
    
    private void CheckLineForStateChange()
    {
        if (!IsStartOrInitialMoveState()) return;
        state = IsTrivialLineDraw()
            ? ShouldDrawTrivialDot()
            : TrivialPathDetectorState.MustDraw;
    }

    private bool IsTrivialLineDraw() =>  lastX == firstX && lasty == firstY;
    
    private bool IsStartOrInitialMoveState() => 
        state is TrivialPathDetectorState.Start or TrivialPathDetectorState.InitialMoveTo;


    private TrivialPathDetectorState ShouldDrawTrivialDot() =>
        graphicsState.CurrentState().LineCap == LineCap.Round
            ? TrivialPathDetectorState.MustDraw
            : TrivialPathDetectorState.ShouldNotDraw;



    public void MoveTo(double x, double y)
    {
        CurrentShape().MoveTo(x, y);
        SetFirst(x,y);
    }

    public void LineTo(double x, double y)
    {
        CurrentShape().LineTo(x, y);
        SetLast(x,y);
    }

    public void CurveTo(double control1X, double control1Y, double control2X, double control2Y, double finalX, double finalY)
    {
        CurrentShape().CurveTo(control1X, control1Y, control2X, control2Y, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void CurveToWithoutInitialControl(double control2X, double control2Y, double finalX, double finalY)
    {
        CurrentShape().CurveTo(lastX, lasty, control2X, control2Y, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void CurveToWithoutFinalControl(double control1X, double control1Y, double finalX, double finalY)
    {
        CurrentShape().CurveTo(control1X, control1Y, finalX, finalY, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void ClosePath()
    {
        if (IsStartOrInitialMoveState())
            LineTo(firstX, firstY);
        CurrentShape().ClosePath();
        SetLast(firstX, firstY);
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
        if (state is TrivialPathDetectorState.MustDraw)
            CurrentShape().PaintPath(stroke, fill, evenOddFillRule);
        EndPathWithNoOp();
    }

    public void ClipToPath() => CurrentShape().ClipToPath(false);

    public void ClipToPathEvenOdd() => CurrentShape().ClipToPath(true);
}