using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Melville.Fonts;
using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

internal struct FreeTypeOutlineWriter
{
    private readonly IGlyphTarget target;
    private const float scale = 16.0f; 
    
    public FreeTypeOutlineWriter(IGlyphTarget target)
    {
        this.target = target;
    }

    public void Decompose(Outline outline)
    {
        var handle = GCHandle.Alloc(target);
        outline.Decompose(drawHandle, GCHandle.ToIntPtr(handle));
        handle.Free();
    }

    public static Vector2 ScalePoint(in FTVector v) => new Vector2((float)v.X, (float)v.Y)* scale;

    private static readonly OutlineFuncs drawHandle = new(MoveTo, LineTo, ConicTo, CubicTo, 0, 0);
    private static int MoveTo(ref FTVector to, IntPtr user)
    {
        Target(user).MoveTo(ScalePoint(to));
        return 0;
    }

    private static IGlyphTarget Target(IntPtr user) => 
        ((IGlyphTarget)GCHandle.FromIntPtr(user).Target!);

    private static int LineTo(ref FTVector to, IntPtr user)
    {
        Target(user).LineTo(ScalePoint(to));
        return 0;
    }

    private static int ConicTo(ref FTVector control, ref FTVector to, IntPtr user)
    {
        Target(user).CurveTo(ScalePoint(control), ScalePoint(to));
        return 0;
    }

    private static int CubicTo(ref FTVector control1, ref FTVector control2, ref FTVector to, IntPtr user)
    {
        Target(user).CurveTo(ScalePoint(control1), ScalePoint(control2), ScalePoint(to));
        return 0;
    }
}