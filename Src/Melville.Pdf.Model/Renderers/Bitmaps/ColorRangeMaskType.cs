using System;
using System.Collections.Generic;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal readonly struct ColorRangeMaskType : IMaskType
{
    private readonly ByteRange redRange;
    private readonly ByteRange greenRange;
    private readonly ByteRange blueRange;

    // Pdf 2.0 spec sec 8.9.6.4 indicates that color masks are an array of integers
    public ColorRangeMaskType(IReadOnlyList<int> values, int bitsPerComponent, IColorSpace colorSpace)
    {
        if (values.Count < colorSpace.ExpectedComponents * 2)
        {
            redRange = blueRange = greenRange = new ByteRange(0, 0);
            return;
        }

        double maxComponent = (1 << bitsPerComponent) - 1;
        var minColor = InterleavedColor(values, 0, maxComponent, colorSpace);
        var maxColor = InterleavedColor(values, 1, maxComponent, colorSpace);
        redRange = new ByteRange(minColor.RedByte, maxColor.RedByte);
        greenRange = new ByteRange(minColor.GreenByte, maxColor.GreenByte);
        blueRange = new ByteRange(minColor.BlueByte, maxColor.BlueByte);
    }

    private DeviceColor InterleavedColor(IReadOnlyList<int> values, int offset, double maxComponent, IColorSpace colorSpace)
    {
        Span<double> color = stackalloc double[colorSpace.ExpectedComponents];
        for (int i = 0; i < colorSpace.ExpectedComponents; i++)
        {
            color[i] = values[offset + 2 * i] / maxComponent;
        }

        return colorSpace.SetColor(color);
    }

    public unsafe byte AlphaForByte(byte alpha, in byte* maskPixel) =>
        blueRange.IsInRange(maskPixel[0]) &&
        greenRange.IsInRange(maskPixel[1]) &&
        redRange.IsInRange(maskPixel[2])
            ? (byte)0
            : (byte)255;
}

internal readonly struct ByteRange
{
    private readonly byte min;
    private readonly byte max;

    public ByteRange(byte min, byte max)
    {
        (this.min, this.max) = max > min ? (min, max) : (max, min);
    }

    public bool IsInRange(byte value) => value >= min && value <= max;

    public override string ToString() => $"{min} .. {max}";
}