using System.Numerics;
using System.Windows.Threading;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf;

public class DispatcherDrawTarget: IDrawTarget
{
    protected readonly Dispatcher dispatcher;
    private readonly IDrawTarget target;

    public DispatcherDrawTarget(Dispatcher dispatcher, IDrawTarget target)
    {
        this.dispatcher = dispatcher;
        this.target = target;
    }

    public void SetDrawingTransform(Matrix3x2 transform) => 
      dispatcher.Invoke(()=>target.SetDrawingTransform(transform));

    public void MoveTo(double x, double y) =>
        dispatcher.Invoke(() => target.MoveTo(x, y));

    public void LineTo(double x, double y) =>
        dispatcher.Invoke(() => target.LineTo(x, y));

    public void ConicCurveTo(double controlX, double controlY, double finalX, double finalY) =>
        dispatcher.Invoke(() => target.ConicCurveTo(controlX, controlY, finalX, finalY));

    public void CurveTo(
        double control1X, double control1Y, double control2X, double control2Y,
        double finalX, double finalY) =>
        dispatcher.Invoke(() => target.CurveTo(control1X, control1Y, control2X, control2Y, finalX, finalY));


    public void PaintPath(bool stroke, bool fill, bool evenOddFillRule) =>
        dispatcher.Invoke(() => target.PaintPath(stroke, fill, evenOddFillRule));

    public void ClipToPath(bool evenOddRule) =>
        dispatcher.Invoke(() => target.ClipToPath(evenOddRule));

    public void ClosePath() => dispatcher.Invoke(target.ClosePath);
}

public class DispatcherRenderTarget : DispatcherDrawTarget, IRenderTarget
{
    private IRenderTarget target;
    public DispatcherRenderTarget(Dispatcher dispatcher, IRenderTarget target) : base(dispatcher, target)
    {
        this.target = target;
    }

    public IGraphiscState GrapicsStateChange => target.GrapicsStateChange;

    public void EndPath() => dispatcher.Invoke(target.EndPath);

    public void SaveTransformAndClip() => dispatcher.Invoke(target.SaveTransformAndClip);


    public void RestoreTransformAndClip() => dispatcher.Invoke(target.RestoreTransformAndClip);

    public void Transform(in Matrix3x2 newTransform)
    {
        var saveTransform = newTransform;
        dispatcher.Invoke(() => target.Transform(saveTransform));
    }

    public ValueTask RenderBitmap(IPdfBitmap bitmap) =>
        dispatcher.Invoke(() => target.RenderBitmap(bitmap));

    public IDrawTarget CreateDrawTarget() =>
        dispatcher.Invoke(() => new DispatcherDrawTarget(dispatcher, target.CreateDrawTarget()));
}