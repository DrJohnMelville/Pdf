using System.Numerics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

// Pdf standard 2.0 section 8.5.3.2 gives very detailed rules for the stroking trivial paths
// that are implemented by this wrapper class.
public partial class TrivialPathDetector : IDrawTarget
{
    [DelegateTo]private IDrawTarget innerTarget = null!;
    private IGraphicsState graphicsState = null!;
    private TrivialPathDetectorState state;
    private double firstX;
    private double firstY;

    public TrivialPathDetector With(IDrawTarget innerTarget, IGraphicsState graphicsState)
    {
        this.innerTarget = innerTarget;
        this.graphicsState = graphicsState;
        state = TrivialPathDetectorState.Start;
        return this;
    }
    
    public void MoveTo(double x, double y)
    {
        TryRecordInitialPoint(x, y);
        innerTarget.MoveTo(x, y);
    }

    private void TryRecordInitialPoint(double x, double y)
    {
        if (!IsStartOrInitialMoveState()) return;
        firstX = x;
        firstY = y;
        state = TrivialPathDetectorState.InitialMoveTo;
    }

    private bool IsStartOrInitialMoveState()
    {
        return state is TrivialPathDetectorState.Start or TrivialPathDetectorState.InitialMoveTo;
    }

    public void LineTo(double x, double y)
    {
        CheckLineForStateChange(x, y);
        innerTarget.LineTo(x, y);
    }

    private void CheckLineForStateChange(double x, double y)
    {
        state = IsTrivialLineDraw(x, y)
            ? ShouldDrawTrivialDot()
            : TrivialPathDetectorState.MustDraw;
    }

    private bool IsTrivialLineDraw(double x, double y) => 
        IsStartOrInitialMoveState() && x == firstX && y == firstY;

    private TrivialPathDetectorState ShouldDrawTrivialDot() =>
        graphicsState.CurrentState().LineCap == LineCap.Round
            ? TrivialPathDetectorState.MustDraw
            : TrivialPathDetectorState.ShouldNotDraw;

    public void ConicCurveTo(double controlX, double controlY, double finalX, double finalY)
    {
        state = TrivialPathDetectorState.MustDraw;
        innerTarget.ConicCurveTo(controlX, controlY, finalX, finalY);
    }

    public void CurveTo(
        double control1X, double control1Y, double control2X, double control2Y, 
        double finalX, double finalY)
    {
        state = TrivialPathDetectorState.MustDraw;
        innerTarget.CurveTo(control1X, control1Y, control2X, control2Y, finalX, finalY);
    }

    public void ClosePath()
    {
        if (state is TrivialPathDetectorState.InitialMoveTo)
        {
            LineTo(firstX, firstY);
        }
        innerTarget.ClosePath();
    }

    public void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        if (state is TrivialPathDetectorState.MustDraw)
            innerTarget.PaintPath(stroke, fill, evenOddFillRule);

        TrivialPathDetectorFactory.Return(this);
    }
}