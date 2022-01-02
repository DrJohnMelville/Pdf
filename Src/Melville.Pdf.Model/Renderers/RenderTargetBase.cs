using System.IO.Pipelines;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

public interface IRenderTarget<TTypeface>
{
    IGraphiscState<TTypeface> GrapicsStateChange { get; }
    void MoveTo(double x, double y);
    void LineTo(double x, double y);
    void CurveTo(double control1X, double control1Y, double control2X, double control2Y, 
        double finalX, double finalY);
    void ClosePath();
    void PaintPath(bool stroke, bool fill, bool evenOddFillRule);
    void EndPath();

    void SaveTransformAndClip();
    void RestoreTransformAndClip();
    void Transform(in Matrix3x2 newTransform);
    void CombineClip(bool evenOddRule);

    ValueTask RenderBitmap(IPdfBitmap bitmap);

    public void SetBuiltInFont(PdfName name, double size);
    (double width, double height) RenderGlyph(byte b);
}

public abstract class RenderTargetBase<T, TTypeface>
{
    protected T Target { get; }
    protected GraphicsStateStack<TTypeface> State { get; }
    protected PdfPage Page { get; }

    public IGraphiscState<TTypeface> GrapicsStateChange => State;

    protected RenderTargetBase(T target, GraphicsStateStack<TTypeface> state, PdfPage page)
    {
        Target = target;
        State = state;
        Page = page;
    }

    protected void MapUserSpaceToBitmapSpace(PdfRect rect, double xPixels, double yPixels)
    {
        var xform = Matrix3x2.CreateTranslation((float)-rect.Left, (float)-rect.Bottom) *
                    Matrix3x2.CreateScale((float)(xPixels/rect.Width), (float)(-yPixels/rect.Height)) *
                    Matrix3x2.CreateTranslation(0, (float)yPixels);
        State.ModifyTransformMatrix(xform);
        Transform(xform);
    }

    public abstract void Transform(in Matrix3x2 newTransform);
}

public static class RenderTargetOperations
{
    public static async ValueTask RenderTo<TTypeface>(
        this IHasPageAttributes page, IRenderTarget<TTypeface> target) =>
        await new ContentStreamParser(
                new RenderEngine<TTypeface>(page, target))
            .Parse(
                PipeReader.Create(
                    await page.GetContentBytes()));
}