using System;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public class FreeTypeOutlineWriter
{
    private readonly IDrawTarget target;

    public FreeTypeOutlineWriter(IDrawTarget target)
    {
        this.target = target;
    }

    public OutlineFuncs DrawHandle() => new(MoveTo, LineTo, ConicTo, CubicTo, 0, 0);

    private int MoveTo(ref FTVector to, IntPtr user)
    {
        target.MoveTo(to.X, to.Y);
        return 0;
    }

    private int LineTo(ref FTVector to, IntPtr user)
    {
        target.LineTo(to.X, to.Y);
        return 0;
    }

    private int ConicTo(ref FTVector control, ref FTVector to, IntPtr user)
    {
        target.ConicCurveTo(control.X,control.Y, to.X, to.Y);
        return 0;
    }

    private int CubicTo(ref FTVector control1, ref FTVector control2, ref FTVector to, IntPtr user)
    {
        target.CurveTo(control1.X, control1.Y, control2.X, control2.Y, to.X, to.Y);
        return 0;
    }
}