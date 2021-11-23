using System;
using System.Text.Json;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.Colors;

public record struct DeviceColor(double Red, double Green, double Blue)
{
    public static readonly DeviceColor Black = new(0, 0, 0);
}

public interface IColorSpace
{
    public DeviceColor SetColor(ReadOnlySpan<double> newColor);
}

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