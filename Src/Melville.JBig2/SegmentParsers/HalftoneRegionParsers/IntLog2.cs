using System.Numerics;
using System.Runtime.CompilerServices;

namespace Melville.JBig2.SegmentParsers.HalftoneRegionParsers;

public static class IntLog
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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