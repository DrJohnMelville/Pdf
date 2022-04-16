using System;
using Melville.Icc.Model.Tags;

namespace Melville.Pdf.Model.Renderers.Colors;

public class XyzToDeviceColor : IColorTransform
{
    public static DeviceColor Transform(in ReadOnlySpan<float> input)
    {
        Span<float> intermed = stackalloc float[3];
        Instance.Transform(input, intermed);
        return new DeviceColor(
            intermed[0],
            intermed[1],
            intermed[2]
        );
    }

    public static readonly IColorTransform Instance = new XyzToDeviceColor();
    private XyzToDeviceColor(){}

    public int Inputs => 3;
    public int Outputs => 3;
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        transformMatrix.PostMultiplyBy(input, output);
        for (int i = 0; i < 3; i++)
        {
            output[i] = GammaCorrect(output[i]);
        }
    }

    private readonly Matrix3x3 transformMatrix = new(
        3.2404542f, -1.5371385f, -0.4985314f,
        -0.9692660f, 1.8760108f, 0.0415560f,
        0.0556434f, -0.2040259f, 1.0572252f);

    private float GammaCorrect(float f) =>
        (float)((f <= 0.0031308 ? 12.92*f: 1.055*Math.Pow(f,1/2.4) - 0.055));
}