using System;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Model.Tags;
using SharpFont.PostScript;

namespace Melville.Pdf.Model.Renderers.Colors;

public class LabToXyz : IColorTransform
{
    public static readonly LabToXyz Instance = new(); 
    private LabToXyz()
    {
    }

    public int Inputs => 3;
    public int Outputs => 3;
    private const float epsilon = 0.008856f;
    private const float kappa = 903.3f;
    
    public void Transform(in ReadOnlySpan<float> color, in Span<float> xyz)
    {
        // transform is the Lab To Xyz transform from http://www.brucelindbloom.com/
        var fy = (color[0] + 16f) / 116f;
        var fx = (color[1] / 500f) + fy;
        var fz = fy - (color[2] / 200f);
        
        // notice that these three are out of order in case Color[0] is aliased to xyz[0], which is the norm.
        xyz[1] = D50WhitePoint.Y * YFunc(color[0]);
        xyz[0] = D50WhitePoint.X * XZFunc(fx);
        xyz[2] = D50WhitePoint.Z * XZFunc(fz);
    }
    private float YFunc(float L) => 
        L > kappa * epsilon ? 
            Cube((L + 16f) / 116) : 
            L / kappa;

    private float Cube(float f) => f * f * f;

    private float XZFunc(float val)
    {
        var cube = Cube(val);
        return cube > epsilon ? cube :
            ((116f * val) - 16f) / kappa;
    }
    // Lab profiles used as a PCS in an ICC have to be relative to D50 per the spec
    // if I ever need other whitepoints, make sure the icc profiles use d50.
    private static class D50WhitePoint
    {
        public const float X = .9642f;
        public const float Y = 1f;
        public const float Z = .8251f;
    }
}

