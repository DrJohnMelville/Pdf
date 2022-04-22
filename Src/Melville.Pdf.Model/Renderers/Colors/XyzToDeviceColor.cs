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

    //Use the Bradford adapted sRGB matrix with a D50 white point from
    //http://www.brucelindbloom.com/ 
    private readonly Matrix3x3 transformMatrix = new(
            1.9624274f, -0.6105343f, -0.3413404f,
             -0.9787684f,  1.9161415f,  0.0334540f,
             0.0286869f, -0.1406752f,  1.3487655f);

    private float GammaCorrect(float f) =>
        (float)((f <= 0.0031308 ? 12.92*f: 1.055*Math.Pow(f,1/2.4) - 0.055));
}