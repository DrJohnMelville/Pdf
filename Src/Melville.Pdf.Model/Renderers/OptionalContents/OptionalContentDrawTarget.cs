using System;
using Melville.INPC;
using Melville.Parsing.ObjectRentals;

namespace Melville.Pdf.Model.Renderers.OptionalContents;

internal partial class OptionalContentDrawTarget : IDrawTarget, IDisposable, IClearable
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
    public void Clear()
    {
        // needs to be defined to prevent recursion
    }
}