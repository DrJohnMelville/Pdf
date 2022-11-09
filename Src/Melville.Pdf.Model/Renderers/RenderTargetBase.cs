using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;


public interface IDrawTarget
{
    void SetDrawingTransform(in Matrix3x2 transform);
    void MoveTo(double x, double y);
    void LineTo(double x, double y);
    void ConicCurveTo(double controlX, double controlY, double finalX, double finalY);
    void CurveTo(double control1X, double control1Y, double control2X, double control2Y,
        double finalX, double finalY);
    void ClosePath();
    void PaintPath(bool stroke, bool fill, bool evenOddFillRule);
    void ClipToPath(bool evenOddRule);
}

public interface IRenderTarget: IDrawTarget, IDisposable
{
    IGraphicsState GraphicsState { get; }
    void EndPath();
    ValueTask RenderBitmap(IPdfBitmap bitmap);
    IDrawTarget CreateDrawTarget();
    void SetBackgroundRect(in PdfRect rect, double width, double height, in Matrix3x2 transform);
    void MapUserSpaceToBitmapSpace(in PdfRect rect, double xPixels, double yPixels, in Matrix3x2 adjustOutput);
    void CloneStateFrom(GraphicsState priorState);
    IRealizedFont WrapRealizedFont(IRealizedFont font) => font;
}

public abstract partial class RenderTargetBase<T, TState>: IRenderTarget
   where TState:GraphicsState, new()
{
    protected T Target { get; }
    protected GraphicsStateStack<TState> State { get; } = new();
    
    public IGraphicsState GraphicsState => State;
    public void SaveGraphicsState() => State.SaveGraphicsState();

    public void RestoreGraphicsState() => State.RestoreGraphicsState();

    protected RenderTargetBase(T target)
    {
        Target = target;
    }
    

    public void MapUserSpaceToBitmapSpace(in PdfRect rect, double xPixels, double yPixels, in Matrix3x2 adjustOutput)
    {
        var xform = adjustOutput *
                    Matrix3x2.CreateTranslation((float)-rect.Left, (float)-rect.Bottom) *
                    Matrix3x2.CreateScale((float)(xPixels / rect.Width), (float)(-yPixels / rect.Height)) *
                    Matrix3x2.CreateTranslation(0, (float)yPixels);
        GraphicsState.ModifyTransformMatrix(xform);
        GraphicsState.StoreInitialTransform();
    }
    
    #region Draw Shapes

    public abstract IDrawTarget CreateDrawTarget();

    protected IDrawTarget? currentShape = null;
    
    [DelegateTo()]
    private IDrawTarget CurrentShape() => currentShape ??= CreateDrawTarget();

    public virtual void ClipToPath(bool evenOddRule) => CurrentShape().ClipToPath(evenOddRule);
    
    public void EndPath()
    {
        (currentShape as IDisposable)?.Dispose();
        currentShape = null;
    }
    #endregion

    public void CloneStateFrom(GraphicsState priorState)
    {
        if (priorState is TState ts) State.StronglyTypedCurrentState().CopyFrom(ts);
        State.StronglyTypedCurrentState().ResetTransformMatrix();
            
    }

    public virtual void Dispose()
    {
        State.Dispose();
    }

    public abstract ValueTask RenderBitmap(IPdfBitmap bitmap);

    public abstract void SetBackgroundRect(
        in PdfRect rect, double width, double height, in Matrix3x2 transform);
}