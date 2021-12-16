using System;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.Colors;

public class DeviceGray : IColorSpace
{
    public static IColorSpace Instance = new DeviceGray();
    public static IColorSpace InvertedInstance = new InvertedDeviceGray();

    public virtual DeviceColor SetColor(in ReadOnlySpan<double> newColor)
    {
        if (newColor.Length != 1)
            throw new PdfParseException("Wrong number of color parameters");
        double value = newColor[0];
        return new DeviceColor(value, value, value);
    }
    public DeviceColor DefaultColor() => DeviceColor.Black;
    public DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor) =>
        this.SetColorSingleFactor(newColor, 1.0 / 255.0);
}

public class InvertedDeviceGray: DeviceGray
{
    public override DeviceColor SetColor(in ReadOnlySpan<double> newColor) => 
        base.SetColor(stackalloc double[]{1.0 - newColor[0]});
}