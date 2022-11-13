using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.OptionalContents;

internal partial class OptionalContentDrawTarget : IDrawTarget, IDisposable
{
    private OptionalContentCounter parent = null!;
    [DelegateTo] private IDrawTarget target = null!;

    public OptionalContentDrawTarget With(OptionalContentCounter parent, IDrawTarget target)
    {
        this.parent = parent;
        this.target = target;
        return this;
    }
        
    public void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        var show = !parent.IsHidden;
        target.PaintPath(show && stroke, show && fill, evenOddFillRule);
    }

    public void Dispose()
    {
        parent.ReturnDrawTarget(this);
    }
}