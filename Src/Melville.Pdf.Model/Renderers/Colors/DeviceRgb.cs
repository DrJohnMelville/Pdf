using System;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Colors;

public class DeviceRgb : IColorSpace
{
    public static IColorSpace Instance = new DeviceRgb();

    public DeviceColor SetColor(in ReadOnlySpan<double> newColor)
    {
        if (newColor.Length != 3)
            throw new PdfParseException("Wrong number of color parameters");
        return new DeviceColor(newColor[0], newColor[1], newColor[2]);
    }
    public DeviceColor DefaultColor() => DeviceColor.Black;
    public DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor) =>
        this.SetColorSingleFactor(newColor, 1.0 / 255.0);
    public int ExpectedComponents => 3;
    
    private ClosedInterval[] outputIntervals = { new(0, 1), new(0, 1), new(0, 1)};
    public ClosedInterval[] DefaultOutputIntervals(int bitsPerComponent) => outputIntervals;
}