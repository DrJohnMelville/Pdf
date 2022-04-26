using System;
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
    public void Transform(in ReadOnlySpan<float> color, in Span<float> xyz)
    {
        var scaledL = (color[0] + 16f) / 116f;

        xyz[0] = D50WhitePoint.X * decFunc(scaledL + (color[1] / 500f));
        xyz[1] = D50WhitePoint.Y * decFunc(scaledL);
        xyz[2] = D50WhitePoint.Z * decFunc(scaledL - (color[2] / 200f));
    }


    float decFunc(float f) =>
        f >= sigma ? f * f * f : (f / (3 * sigma * sigma)) + (4f / 29f);

    private const float sigma = 6f / 29f;

    // Lab profiles used as a PCS in an ICC have to be relative to D50 per the spec
    // if I ever need other whitepoints, make sure the icc profiles use d50.
    private static class D50WhitePoint
    {
        public const float X = .9642f;
        public const float Y = 1f;
        public const float Z = .8251f;
    }
}

