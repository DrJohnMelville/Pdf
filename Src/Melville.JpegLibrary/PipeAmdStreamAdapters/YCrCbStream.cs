using Melville.INPC;

namespace Melville.JpegLibrary.PipeAmdStreamAdapters;

internal static class ConvertYCrCb
{
    public static unsafe byte[] Convert(byte[] data, int components)
    {
        fixed (byte* sourceBase = data)
        {
            var sourcePtr = sourceBase;
            var destPtr = sourceBase;
            for (int i = 0; i < components / 3; i++)
            {
                (*destPtr++, *destPtr++, *destPtr++) = YCbCrToRgbConverter.YCbCrToRGB(
                    *sourcePtr++, *sourcePtr++, *sourcePtr++);
            }
        }
        return data;
    }
}