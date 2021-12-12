using System;
using Melville.Icc.Model.Tags;

namespace Melville.Pdf.Model.Renderers.Colors;

public static class XyzToDeviceColor 
{
    private static readonly Matrix3x3 transformMatrix = new(
        3.2404542f, -1.5371385f, -0.4985314f,
        -0.9692660f, 1.8760109f, 0.0415560f,
        0.0556434f, -0.2040259f, 1.0572252f);
    public static DeviceColor Transform(in ReadOnlySpan<float> input)
    {
        Span<float> intermed = stackalloc float[3];
        transformMatrix.PostMultiplyBy(input, intermed);
        return new DeviceColor(
            GammaCorrect(intermed[0]),
            GammaCorrect(intermed[1]),
            GammaCorrect(intermed[2])
        );
    }
    private static float GammaCorrect(float f) =>
        (float)(f <= 0.0031308 ? 12.92*f: 1.055*Math.Pow(f,1/2.4) - 0.055);
}