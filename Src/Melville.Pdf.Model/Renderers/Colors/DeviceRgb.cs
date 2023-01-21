using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Colors;

[StaticSingleton]
internal partial class DeviceRgb : IColorSpace
{
    public DeviceColor SetColor(in ReadOnlySpan<double> newColor)
    {
        if (newColor.Length != 3)
            throw new PdfParseException("Wrong number of color parameters");
        return DeviceColor.FromDoubles(newColor[0], newColor[1], newColor[2]);
    }
    public DeviceColor DefaultColor() => DeviceColor.Black;
    public DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor) =>
        this.SetColorSingleFactor(newColor, 1.0 / 255.0);
    public int ExpectedComponents => 3;
    
    private static ClosedInterval[] outputIntervals = { new(0, 1), new(0, 1), new(0, 1)};
    public ClosedInterval[] ColorComponentRanges(int bitsPerComponent) => outputIntervals;
}