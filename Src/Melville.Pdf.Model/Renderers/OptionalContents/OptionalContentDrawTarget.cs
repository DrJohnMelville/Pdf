using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.OptionalContents;

internal partial class OptionalContentDrawTarget : IDrawTarget, IDisposable
{
    private OptionalContentCounter optionalContentCounter = null!;
    [DelegateTo] private IDrawTarget target = null!;

    public OptionalContentDrawTarget With(OptionalContentCounter optionalContentCounter, IDrawTarget target)
    {
        this.optionalContentCounter = optionalContentCounter;
        this.target = target;
        return this;
    }
        
    public void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        if (!optionalContentCounter.IsHidden) target.PaintPath(stroke, fill, evenOddFillRule);
    }

    public void Dispose() => optionalContentCounter.ReturnDrawTarget(this);
}