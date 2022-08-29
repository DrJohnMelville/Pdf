using System.Diagnostics;

namespace Melville.JpegLibrary.PipeAmdStreamAdapters;

public class YCCKStream: ConvertingStream
{
    public YCCKStream(byte[] data, int length) : base(data, length, 4)
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
                var (red,green,blue) = YCbCrToRgbConverter.YCbCrToRGB(
                    sourcePtr[0], sourcePtr[1], sourcePtr[2]);
                sourcePtr += 3;
                *destPtr++ = (byte)(255 - red);
                *destPtr++ = (byte)(255 - green);
                *destPtr++ = (byte)(255 - blue);
                *destPtr++ = *sourcePtr++;
            }
        }
    }
}