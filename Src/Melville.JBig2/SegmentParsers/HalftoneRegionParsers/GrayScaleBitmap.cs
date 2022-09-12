using System.Diagnostics;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.Segments;

namespace Melville.JBig2.SegmentParsers.HalftoneRegionParsers;

public readonly ref struct GrayScaleBitmap
{
    private readonly Span<int> data;
    private readonly int width;
    private int Height => data.Length / width;

    public GrayScaleBitmap(Span<int> data, int width)
    {
        this.data = data;
        this.width = width;
        Debug.Assert(data.Length % width == 0);
    }

    public void CopyBinaryBitmap(IBinaryBitmap source, int bitValue)
    {
        AssertCorrectBitmapSize(source);
        var pos = 0;
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                data[pos++] = source[i, j] ? bitValue : 0;
            }
        }
    }

    [Conditional("DEBUG")]
    private void AssertCorrectBitmapSize(IBinaryBitmap source)
    {
        Debug.Assert(source.Height == Height);
        Debug.Assert(width == source.Width);
    }

    public void ProcessBinaryBitmap(IBinaryBitmap source, int bitMask, int priorBitMask)
    {
        AssertCorrectBitmapSize(source);
        var pos = 0;
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (BitOperations.CheckBit(data[pos], priorBitMask) ^ source[i,j])
                {
                    data[pos] |= bitMask;
                }
                pos++;
            }
        }
    }

    public int this[int row, int col] => data[(row * width) + col];
}