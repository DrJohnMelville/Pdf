using System;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public class FreeTypeOutlineWriter
{
    private readonly IDrawTarget target;
    private const double scale = 16.0; 
    
    public FreeTypeOutlineWriter(IDrawTarget target)
    {
        this.target = target;
    }

    public OutlineFuncs DrawHandle() => new(MoveTo, LineTo, ConicTo, CubicTo, 0, 0);

    private int MoveTo(ref FTVector to, IntPtr user)
    {
        target.MoveTo(to.X*scale, to.Y*scale);
        return 0;
    }

    private int LineTo(ref FTVector to, IntPtr user)
    {
        target.LineTo(to.X*scale, to.Y*scale);
        return 0;
    }

    private int ConicTo(ref FTVector control, ref FTVector to, IntPtr user)
    {
        target.ConicCurveTo(control.X*scale,control.Y*scale, to.X*scale, to.Y*scale);
        return 0;
    }

    private int CubicTo(ref FTVector control1, ref FTVector control2, ref FTVector to, IntPtr user)
    {
        target.CurveTo(control1.X*scale, control1.Y*scale, control2.X*scale, control2.Y*scale, 
            to.X*scale, to.Y*scale);
        return 0;
    }
}