using System;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Colors;

internal class DeviceGray : IColorSpace
{
    public static IColorSpace Instance = new DeviceGray();
    public static IColorSpace InvertedInstance = new InvertedDeviceGray();

    public virtual DeviceColor SetColor(in ReadOnlySpan<double> newColor)
    {
        if (newColor.Length != 1)
            throw new PdfParseException("Wrong number of color parameters");
        double value = newColor[0];
        return DeviceColor.FromDoubles(value, value, value);
    }
    public DeviceColor DefaultColor() => DeviceColor.Black;
    public DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor) =>
        this.SetColorSingleFactor(newColor, 1.0 / 255.0);
    public int ExpectedComponents => 1;
    private static ClosedInterval[] outputIntervals = { new(0, 1) };
    public ClosedInterval[] ColorComponentRanges(int bitsPerComponent) => outputIntervals;

}

internal class InvertedDeviceGray: DeviceGray
{
    public override DeviceColor SetColor(in ReadOnlySpan<double> newColor) => 
        base.SetColor(stackalloc double[]{1.0 - newColor[0]});
}