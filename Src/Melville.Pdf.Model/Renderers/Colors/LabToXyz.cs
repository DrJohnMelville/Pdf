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

        xyz[0] = xN * decFunc(scaledL + (color[1] * 256f/ 500f));
        xyz[1] = yN * decFunc(scaledL);
        xyz[2] = zN * decFunc(scaledL - (color[2] * 256f/ 200f));
    }

    private const float xN = .950489f;
    private const float yN = 1f;
    private const float zN = 1.088840f;

    float decFunc(float f) =>
        f >= 6f / 29f ? f * f * f : (f / (3 * sigma * sigma)) + (4f / 29f);

    private const float sigma = 6f / 29f;
}