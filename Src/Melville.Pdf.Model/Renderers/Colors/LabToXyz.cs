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

        xyz[0] = D65WhitePoint.X * decFunc(scaledL + (color[1] / 500f));
        xyz[1] = D65WhitePoint.Y * decFunc(scaledL);
        xyz[2] = D65WhitePoint.Z * decFunc(scaledL - (color[2] / 200f));
    }


    float decFunc(float f) =>
        f >= sigma ? f * f * f : (f / (3 * sigma * sigma)) + (4f / 29f);

    private const float sigma = 6f / 29f;

    private static class D65WhitePoint
    {
        public const float X = .950489f;
        public const float Y = 1f;
        public const float Z = 1.088840f;
    }
}

