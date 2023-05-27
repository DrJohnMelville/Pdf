using System.Numerics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.ImageExtractor;

[FromConstructor]
internal partial class ImageExtractorTarget: RenderTargetBase<IList<IExtractedBitmap>, UncoloredGraphicsState>
{
    [FromConstructor] private readonly int page;
    public override ValueTask RenderBitmapAsync(IPdfBitmap bitmap)
    {
        Target.Add(new WrapNonExtractedBitmap(bitmap, 
            this.GraphicsState.CurrentState().TransformMatrix, page));
        return ValueTask.CompletedTask;
    }

    public override void SetBackgroundRect(in PdfRect rect, double width, double height, in Matrix3x2 transform)
    {
    }

    public override IDrawTarget CreateDrawTarget() => NullDrawTarget.Instance;
}