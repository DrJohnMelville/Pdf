using System;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.Colors;

public class DeviceGray : IColorSpace
{
    public static IColorSpace Instance = new DeviceGray();

    public DeviceColor SetColor(ReadOnlySpan<double> newColor)
    {
        if (newColor.Length != 1)
            throw new PdfParseException("Wrong number of color parameters");
        double value = newColor[0];
        return new DeviceColor(value, value, value);
    }
}