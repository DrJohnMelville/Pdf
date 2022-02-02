using System;
using System.IO.Pipelines;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

public interface IDrawTarget
{
    void SetDrawingTransform(Matrix3x2 transform);
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
    IGraphiscState GrapicsStateChange { get; }
    void EndPath();
    void SaveTransformAndClip();
    void RestoreTransformAndClip();
    void Transform(in Matrix3x2 newTransform);
    ValueTask RenderBitmap(IPdfBitmap bitmap);
    IDrawTarget CreateDrawTarget();
}

public abstract partial class RenderTargetBase<T>: IDrawTarget, IDisposable
{
    protected T Target { get; }
    protected GraphicsStateStack State { get; } = new GraphicsStateStack();

    public IGraphiscState GrapicsStateChange => State;

    protected RenderTargetBase(T target)
    {
        Target = target;
    }

    public void Dispose()
    {
        State.Dispose();
    }

    
    protected void MapUserSpaceToBitmapSpace(in PdfRect rect, in Matrix3x2 adjustOutput, double xPixels, double yPixels)
    {
        var xform = adjustOutput *
                    Matrix3x2.CreateTranslation((float)-rect.Left, (float)-rect.Bottom) *
                    Matrix3x2.CreateScale((float)(xPixels / rect.Width), (float)(-yPixels / rect.Height)) *
                    Matrix3x2.CreateTranslation(0, (float)yPixels);
        State.ModifyTransformMatrix(xform);
        Transform(xform);
    }

    public abstract void Transform(in Matrix3x2 newTransform);
    
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
}