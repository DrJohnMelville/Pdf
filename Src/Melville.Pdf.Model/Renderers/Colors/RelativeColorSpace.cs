using System;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Colors;

public class RelativeColorSpace : IColorSpace
{
    private IColorSpace source;
    private PdfFunction function;

    public RelativeColorSpace(IColorSpace source, PdfFunction function)
    {
        this.source = source;
        this.function = function;
    }

    public DeviceColor SetColor(in ReadOnlySpan<double> newColor)
    {
        Span<double> target = stackalloc double[function.Range.Length];
        function.Compute(newColor, target);
        return source.SetColor(target);
    }

    public DeviceColor DefaultColor()
    {
        var colorCount = ExpectedComponents;
        Span<double> defaultColor = stackalloc double[colorCount];
        for (int i = 0; i < colorCount; i++)
        {
            defaultColor[i] = 1;
        }
        return SetColor(defaultColor);
    }

    public DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor) =>
        this.SetColorSingleFactor(newColor, 1.0 / 255.0);

    public int ExpectedComponents => function.Domain.Length;
    public ClosedInterval[] DefaultOutputIntervals(int bitsPerComponent) => 
        function.Domain;

}