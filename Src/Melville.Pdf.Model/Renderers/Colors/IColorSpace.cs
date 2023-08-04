
using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Colors;

/// <summary>
/// Represented a PDF Colorspace
/// </summary>
public interface IColorSpace
{
    /// <summary>
    /// Convert a color in the colorspace to the device color.
    /// </summary>
    /// <param name="newColor">Color in the colorspace.</param>
    /// <returns>DEviceColor corresponding to the color in the colorspace.</returns>
    DeviceColor SetColor(in ReadOnlySpan<double> newColor);

    /// <summary>
    /// The default color for this colorspace.
    /// </summary>
    DeviceColor DefaultColor();

    /// <summary>
    /// Set a color value using bytes as the input
    /// </summary>
    /// <param name="newColor">Bytes representing the color.</param>
    /// <returns>The represented color.</returns>
    DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor);

    /// <summary>
    /// Number of components needed for this color.
    /// </summary>
    public int ExpectedComponents { get; }

    /// <summary>
    /// Gives the range of each color component, for a given bits per component
    /// </summary>
    /// <param name="bitsPerComponent">Bits per component of the input</param>
    /// <returns>Array of the range for each color components</returns>
    ClosedInterval[] ColorComponentRanges(int bitsPerComponent);

    /// <summary>
    /// If a indexed color space is set to a default of device color space, the underlying
    /// colorspace is the default. 
    /// </summary>
    /// <returns>The colorspace that should be used as the default colorspace.</returns>
    public IColorSpace AsValidDefaultColorSpace() => this;
}

internal static class IColorSpaceOperations
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

    /// <summary>
    /// Convert a color in the colorspace to the device color.
    /// </summary>
    /// <param name="target">The colorspace to compute a color in.</param>
    /// <param name="newColor">Color in the colorspace.</param>
    /// <returns>DEviceColor corresponding to the color in the colorspace.</returns>
    public static DeviceColor SetColor(this IColorSpace target, IReadOnlyList<double> newColor)
    {
        Span<double> elts = stackalloc double[newColor.Count];
        for (int i = 0; i < elts.Length; i++)
        {
            elts[i] = newColor[i];
        }
        return target.SetColor(elts);
    }

}