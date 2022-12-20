using System.Diagnostics.Contracts;

namespace Melville.CCITT;

internal record struct LineComparison(int A1, int A2, int B1, int B2)
{
    public bool CanPassEncode => B2 < A1;
    
    public bool CanVerticalEncode => Math.Abs(VerticalEncodingDelta) <= 3;
    public int VerticalEncodingDelta => A1 - B1;

    [Pure]
    public int FirstHorizontalDelta(int a0) => A1 - Math.Max(a0, 0);
    [Pure]
    public int SecondHorizontalDelta => A2 - A1;
}

