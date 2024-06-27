using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

internal struct FreeTypeOutlineWriter
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
        double y = to.Y*scale;
        Target(user).MoveTo(new Vector2((float)(to.X*scale), (float)y));
        return 0;
    }

    private static IDrawTarget Target(IntPtr user) => 
        ((IDrawTarget)GCHandle.FromIntPtr(user).Target!);

    private static int LineTo(ref FTVector to, IntPtr user)
    {
        double y = to.Y*scale;
        Target(user).LineTo(new Vector2((float)(to.X*scale), (float)y));
        return 0;
    }

    private static int ConicTo(ref FTVector control, ref FTVector to, IntPtr user)
    {
        double controlY = control.Y*scale;
        double finalX = to.X*scale;
        double finalY = to.Y*scale;
        Target(user).ConicCurveTo(new Vector2((float)(control.X*scale), (float)controlY), 
            new Vector2((float)finalX, (float)finalY));
        return 0;
    }

    private static int CubicTo(ref FTVector control1, ref FTVector control2, ref FTVector to, IntPtr user)
    {
        double control1Y = control1.Y*scale;
        double control2X = control2.X*scale;
        double control2Y = control2.Y*scale;
        double finalX = to.X*scale;
        double finalY = to.Y*scale;
        Target(user).CurveTo(
            new Vector2((float)(control1.X*scale), (float)control1Y), 
            new Vector2((float)control2X, (float)control2Y), 
            new Vector2((float)finalX, (float)finalY));
        return 0;
    }
}