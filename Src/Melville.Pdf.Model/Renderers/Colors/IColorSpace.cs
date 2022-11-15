
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Colors;

public interface IColorSpace
{
    DeviceColor SetColor(in ReadOnlySpan<double> newColor);
    DeviceColor DefaultColor();
    DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor);
    public int ExpectedComponents { get; }
    ClosedInterval[] ColorComponentRanges(int bitsPerComponent);
    // Pdf 2.0 Spec defines which color spaces must substitute a different color space
    public IColorSpace AsValidDefaultColorSpace() => this;
}

public static class IColorSpaceOperations
{
    public static DeviceColor SetColorSingleFactor(this IColorSpace cs, in ReadOnlySpan<byte> data, double factor)
    {
        Span<double> doubles = stackalloc double[data.Length];
        for (int i = 0; i < doubles.Length; i++)
        {
            doubles[i] = factor * data[i];
        }

        return cs.SetColor(doubles);
    }
}