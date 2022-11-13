using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

internal enum TrivialPathDetectorState
{
    Start, InitialMoveTo, MustDraw, ShouldNotDraw
}

// Pdf standard 2.0 section 8.5.3.2 gives very detailed rules for the stroking trivial paths
// this class implements them as a state machine
public abstract partial class TrivialPathStateMachine
{
    [FromConstructor] private readonly IGraphicsState graphicsState;
    private TrivialPathDetectorState state = TrivialPathDetectorState.Start;
    protected double FirstX { get; set; }
    protected double FirstY { get; set; }
    protected double CurrentX { get; set; }
    protected double CurrentY { get; set; }

    protected void ResetState()
    {
        state = TrivialPathDetectorState.Start;
        FirstX = FirstY = CurrentX = CurrentY = 0.0;
    }

    protected void RegisterInitialMove(double x, double y)
    {
        (CurrentX, CurrentY) = (FirstX, FirstY) = (x, y);
        TryTransitionToInitialMoveState();
    }

    private void TryTransitionToInitialMoveState()
    {
        if (state is TrivialPathDetectorState.Start) state = TrivialPathDetectorState.InitialMoveTo;
    }

    protected void RegisterDrawOperationLastPoint(double x, double y)
    {
        (CurrentX, CurrentY) = (x, y);
        CheckLineForStateChange();
    }

    private void CheckLineForStateChange()
    {
        if (!IsStartOrInitialMoveState()) return;
        state = IsTrivialLineDraw()
            ? ShouldDrawTrivialDot()
            : TrivialPathDetectorState.MustDraw;
    }

    private bool IsTrivialLineDraw() =>  CurrentX == FirstX && CurrentY == FirstY;

    protected bool IsStartOrInitialMoveState() => 
        state is TrivialPathDetectorState.Start or TrivialPathDetectorState.InitialMoveTo;


    private TrivialPathDetectorState ShouldDrawTrivialDot() =>
        graphicsState.CurrentState().LineCap == LineCap.Round
            ? TrivialPathDetectorState.MustDraw
            : TrivialPathDetectorState.ShouldNotDraw;

    protected bool ShouldPaintPath() => state is TrivialPathDetectorState.MustDraw;
}