using System.IO;
using System.IO.Pipelines;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

public interface IRenderTarget
{
    IGraphiscState GrapicsStateChange { get; }
    void MoveTo(double x, double y);
    void LineTo(double x, double y);
    void CurveTo(double control1X, double control1Y, double control2X, double control2Y, 
        double finalX, double finalY);
    void ClosePath();
    void StrokePath();
    void EndPathWithNoOp();
}

public abstract class RenderTargetBase<T>
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
                    Matrix3x2.CreateScale((float)(xPixels/rect.Width), (float)(-yPixels/rect.Height)) *
                    Matrix3x2.CreateTranslation(0, (float)yPixels);
        State.ModifyTransformMatrix(xform);
    }
}

public static class RenderTargetOperations
{
    public static async ValueTask RenderTo(this PdfPage page, IRenderTarget target) =>
        await new ContentStreamParser(
                new RenderEngine(page, target))
            .Parse(
                PipeReader.Create(
                    await page.GetContentBytes()));
}