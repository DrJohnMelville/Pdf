using System.Diagnostics;
using Melville.INPC;

namespace Melville.JpegLibrary.PipeAmdStreamAdapters;

internal abstract partial class ConvertingStream : RentedArrayReadingStream
{
    [FromConstructor] private int components;
    
    protected override int CopyBytes(Span<byte> source, Span<byte> destination)
    {
        var minLen = Math.Min(source.Length, destination.Length) / components; // integer division
        ConvertColors(source, destination, minLen);
        return minLen * components;
    }

    protected abstract void ConvertColors(ReadOnlySpan<byte> ycbcr, Span<byte> rgb, int count);
}

internal class YCrCbStream: ConvertingStream
{
    public YCrCbStream(byte[] data, int length) : base(data, length, 3)
    {
    }

    protected override unsafe void ConvertColors(ReadOnlySpan<byte> ycbcr, Span<byte> rgb, int count)
    {
        fixed(byte* sourceBase = ycbcr)
        fixed (byte* destBase = rgb)
        {
            var sourcePtr = sourceBase;
            var destPtr = destBase;
            for (int i = 0; i < count; i++)
            {
                (*destPtr++,*destPtr++,*destPtr++) = YCbCrToRgbConverter.YCbCrToRGB(
                    *sourcePtr++,*sourcePtr++,*sourcePtr++);
            }
        }
    }
}