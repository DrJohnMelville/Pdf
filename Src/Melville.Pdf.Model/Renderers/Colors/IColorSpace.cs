
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melville.Pdf.Model.Renderers.Colors;

public interface IColorSpace
{
    DeviceColor SetColor(in ReadOnlySpan<double> newColor);
    DeviceColor DefaultColor();
    DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor);
    public int ExpectedComponents { get; }
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