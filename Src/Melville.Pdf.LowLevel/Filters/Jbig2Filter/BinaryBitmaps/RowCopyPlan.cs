
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public readonly record struct RowCopyPlan(
    int FirstSourceBit, int FirstDestBit,
    int WholeBytes, int SuffixBits, CombinationOperator CombinationOperator)
{


    public ByteSplicer PrefixSplicer() => new ByteSplicer(FirstDestBit);
    public ByteSplicer PostSplicer() => new(SuffixBits);

}