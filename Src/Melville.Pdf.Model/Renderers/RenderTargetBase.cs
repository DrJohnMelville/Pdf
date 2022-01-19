using System;
using System.IO.Pipelines;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.FontMappings;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

public interface IDrawTarget
{
    void MoveTo(double x, double y);
    void LineTo(double x, double y);
    void CurveTo(double control1X, double control1Y, double control2X, double control2Y,
        double finalX, double finalY);
    void ClosePath();
    void PaintPath(bool stroke, bool fill, bool evenOddFillRule);
    void ClipToPath(bool evenOddRule);
}

public interface IRenderTarget: IDrawTarget
{
    IGraphiscState GrapicsStateChange { get; }
    void EndPath();
    void SaveTransformAndClip();
    void RestoreTransformAndClip();
    void Transform(in Matrix3x2 newTransform);
    ValueTask RenderBitmap(IPdfBitmap bitmap);
    public ValueTask SetFont(IFontMapping font, double size);
}

public abstract partial class RenderTargetBase<T>: IDrawTarget
{
    protected T Target { get; }
    protected GraphicsStateStack State { get; }
    protected PdfPage Page { get; }

    public IGraphiscState GrapicsStateChange => State;

    protected RenderTargetBase(T target, GraphicsStateStack state, PdfPage page)
    {
        Target = target;
        State = state;
        Page = page;
    }

    protected void MapUserSpaceToBitmapSpace(PdfRect rect, double xPixels, double yPixels)
    {
        var xform = Matrix3x2.CreateTranslation((float)-rect.Left, (float)-rect.Bottom) *
                    Matrix3x2.CreateScale((float)(xPixels / rect.Width), (float)(-yPixels / rect.Height)) *
                    Matrix3x2.CreateTranslation(0, (float)yPixels);
        State.ModifyTransformMatrix(xform);
        Transform(xform);
    }

    public abstract void Transform(in Matrix3x2 newTransform);

    #region TextRendering

    public Matrix3x2 CharacterPositionMatrix() =>
        (GlyphAdjustmentMatrix() *
         State.CurrentState().TextMatrix);

    private Matrix3x2 GlyphAdjustmentMatrix() => new(
        (float)State.CurrentState().HorizontalTextScale / 100, 0,
        0, -1,
        0, (float)State.CurrentState().TextRise);

    #endregion

    #region Draw Shapes

    protected abstract IDrawTarget CreateDrawTarget();

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

public static class RenderTargetOperations
{
    public static async ValueTask RenderTo(
        this IHasPageAttributes page, IRenderTarget target, FontReader fonts) =>
        await new ContentStreamParser(
                new RenderEngine(page, target, fonts))
            .Parse(
                PipeReader.Create(
                    await page.GetContentBytes()));
}