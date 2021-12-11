using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using Melville.Icc.Model;
using Melville.Icc.Model.Tags;
using Melville.Icc.Parser;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.Colors;

[Obsolete]
public class DeviceCmyk : IColorSpace
{
    public static IColorSpace Instance = new DeviceCmyk();
#warning -- obviously cannot source profiles off of my local hard drive. -- also need to consider render intents.
    public IColorTransform source =
        LoadProfile(@"C:\Users\jmelv\Documents\Scratch\ICC_Profile_Registry_01_11_2013\CGATS21_CRPC3.icc")
            .TransformTo(LoadProfile(@"C:\Users\jmelv\Documents\Scratch\sRGB_v4_ICC_preference.icc"));

    private static IccProfile LoadProfile(string profileFile) => 
        new IccParser(PipeReader.Create(File.OpenRead(profileFile))).ParseAsync().GetAwaiter().GetResult();

    public DeviceColor SetColor(ReadOnlySpan<double> newColor)
    {
        if (newColor.Length != 4)
            throw new PdfParseException("Wrong number of color parameters");
        Span<float> input = stackalloc float[]
        {
            (float)newColor[0], 
            (float)newColor[1], 
            (float)newColor[2], 
            (float)newColor[3], 
        };
        Span<float> output = stackalloc float[3];
        source.Transform(input, output);
       return new DeviceColor(output[0], output[1], output[2]);
    }

}

public class IccColorSpace : IColorSpace
{
    private readonly IColorTransform transform;

    public IccColorSpace(IColorTransform transform)
    {
        this.transform = transform;
    }

    public DeviceColor SetColor(ReadOnlySpan<double> newColor)
    {
        if (newColor.Length != transform.Inputs)
            throw new PdfParseException("Incorrect number of color parameters");
        Span<float> inputs = stackalloc float[newColor.Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = (float)newColor[i];
        }

        Span<float> output = stackalloc float[3];
        transform.Transform(inputs, output);
        return new DeviceColor(output[0], output[1], output[2]);
    }
}