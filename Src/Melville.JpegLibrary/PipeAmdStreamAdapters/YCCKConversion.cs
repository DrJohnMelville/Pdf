namespace Melville.JpegLibrary.PipeAmdStreamAdapters;

internal static class YCCKConversion
{
    public static unsafe byte[] ConvertArray(byte[] input, int count)
    {
        fixed (byte* sourceBase = input)
        {
            var sourcePtr = sourceBase;
            var destPtr = sourceBase;
            for (int i = 0; i < (count/4); i++)
            {
                var (red, green, blue) = YCbCrToRgbConverter.YCbCrToRGB(
                    *sourcePtr++, *sourcePtr++, *sourcePtr++);
                *destPtr++ = (byte)(255 - red);
                *destPtr++ = (byte)(255 - green);
                *destPtr++ = (byte)(255 - blue);
                destPtr++;
                sourcePtr++;
            }
        }

        return input;
    }
}