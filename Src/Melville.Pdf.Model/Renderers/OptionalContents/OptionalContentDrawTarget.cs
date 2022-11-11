using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.OptionalContents;

internal partial class OptionalContentDrawTarget : IDrawTarget
{
    [FromConstructor] private readonly OptionalContentTarget parent;
    [DelegateTo] [FromConstructor] private readonly IDrawTarget innerTarget;
        
    public void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        var show = !parent.IsHidden;
        innerTarget.PaintPath(show && stroke, show && fill, evenOddFillRule);
    }
}