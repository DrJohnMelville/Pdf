using System.Numerics;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.TextExtractor;

public interface IExtractedTextTarget
{
    void RenderText(string text, int page, Vector2 start, Vector2 end, double size,
        IRealizedFont font);
}
public class ExtractTextRender :
    RenderTargetBase<IExtractedTextTarget, UncoloredGraphicsState>
{
    public ExtractTextRender(IExtractedTextTarget target) : base(target)
    {
    }

    public override IDrawTarget CreateDrawTarget() => NullDrawTarget.Instance;

    public override ValueTask RenderBitmapAsync(IPdfBitmap bitmap) => ValueTask.CompletedTask;

    public override void SetBackgroundRect(
        in PdfRect rect, double width, double height, in Matrix3x2 transform)
    {
    }

    public override IRealizedFont WrapRealizedFont(IRealizedFont font)
    {
        return base.WrapRealizedFont(font);
    }
}