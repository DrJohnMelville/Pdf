using System;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.Colors;

public class DeviceCmyk : IColorSpace
{
    public static IColorSpace Instance = new DeviceCmyk();

    public DeviceColor SetColor(ReadOnlySpan<double> newColor)
    {
        if (newColor.Length != 4)
            throw new PdfParseException("Wrong number of color parameters");
        var black = newColor[3];
        return new DeviceColor(
            Invert(newColor[0] + black), 
            Invert(newColor[1] + black), 
            Invert(newColor[2] + black));
    }

    private static double Invert(double value) => 1.0 - Math.Min(1.0, value);
}