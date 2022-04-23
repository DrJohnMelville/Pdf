using System;
using System.ComponentModel.DataAnnotations.Schema;
using Melville.Icc.Model.Tags;

namespace Melville.Pdf.Model.Renderers.Colors;

public class XyzToDeviceColor : IColorTransform
{
    private readonly Matrix3x3 xyYXToRGB;
    
    public XyzToDeviceColor(FloatColor whitePoint) :
        
        this(XyYxToRgb(whitePoint)){}

    private static Matrix3x3 XyYxToRgb(FloatColor whitePoint)
    {
        return LinearizedRgbToXyzMatrixFactory.XyzFromSrgb(whitePoint).Inverse();
    }

    private XyzToDeviceColor(Matrix3x3 xyYxToRgb)
    {
        xyYXToRGB = xyYxToRgb;
    }

    public int Inputs => 3;
    public int Outputs => 3;
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        xyYXToRGB.PostMultiplyBy(input, output);
        for (int i = 0; i < 3; i++)
        {
            output[i] = GammaCorrect(output[i]);
        }
    }


    private float GammaCorrect(float f) =>
        (float)((f <= 0.0031308 ? 12.92*f: 1.055*Math.Pow(f,1/2.4) - 0.055));

    public static readonly IColorTransform FromD50 = 
        new XyzToDeviceColor(new FloatColor(.96422f, 1f, .82491f));
    // public static readonly IColorTransform FromD65 = 
    //     new XyzToDeviceColor(new FloatColor(0.95407f,1f, 1.08883f));
}

