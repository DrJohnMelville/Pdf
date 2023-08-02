using System.Numerics;

namespace Melville.JBig2.SegmentParsers.HalftoneRegionParsers;

internal static class IntLog
{
    public static int CeilingLog2Of(uint value)
    {
        var result = BitOperations.Log2(value);
        if (BitOperations.PopCount(value) != 1)
        {
            result++;
        }
        return result;
    }
}