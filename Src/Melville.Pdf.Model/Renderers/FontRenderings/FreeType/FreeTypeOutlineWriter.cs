using System;
using System.Runtime.InteropServices;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public readonly struct FreeTypeOutlineWriter
{
    private readonly IDrawTarget target;
    private const double scale = 16.0; 
    
    public FreeTypeOutlineWriter(IDrawTarget target)
    {
        this.target = target;
    }

    public void Decompose(Outline outline)
    {
        var handle = GCHandle.Alloc(target);
        outline.Decompose(drawHandle, GCHandle.ToIntPtr(handle));
        handle.Free();
    }

    private static readonly OutlineFuncs drawHandle = new(MoveTo, LineTo, ConicTo, CubicTo, 0, 0);
    private static int MoveTo(ref FTVector to, IntPtr user)
    {
        Target(user).MoveTo(to.X*scale, to.Y*scale);
        return 0;
    }

    private static IDrawTarget Target(IntPtr user) => 
        ((IDrawTarget)GCHandle.FromIntPtr(user).Target!);

    private static int LineTo(ref FTVector to, IntPtr user)
    {
        Target(user).LineTo(to.X*scale, to.Y*scale);
        return 0;
    }

    private static int ConicTo(ref FTVector control, ref FTVector to, IntPtr user)
    {
        Target(user).ConicCurveTo(control.X*scale,control.Y*scale, to.X*scale, to.Y*scale);
        return 0;
    }

    private static int CubicTo(ref FTVector control1, ref FTVector control2, ref FTVector to, IntPtr user)
    {
        Target(user).CurveTo(control1.X*scale, control1.Y*scale, control2.X*scale, control2.Y*scale, 
            to.X*scale, to.Y*scale);
        return 0;
    }
}