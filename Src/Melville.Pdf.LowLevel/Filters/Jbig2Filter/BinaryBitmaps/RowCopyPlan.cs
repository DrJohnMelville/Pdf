
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public readonly record struct RowCopyPlan(
    byte FirstSourceBit, byte FirstDestBit,
    int WholeBytes, byte SuffixBits, CombinationOperator CombinationOperator)
{
    public IBulkByteCopy BulkCopier() =>
        (FirstSourceBit == FirstDestBit, CombinationOperator) switch
        {
            (true, CombinationOperator.Replace) => AlignedReplaceBulkCopy.Instance,
            (false, CombinationOperator.Replace) => SourceOffsetBulkCopy.Instance,
            (true, _) => AlignedBulkOperation.Instance,
            _ => SourceOffsetBulkOperation.Instance
        };

    public IBulkByteCopy PrefixCopier() =>
        (FirstSourceBit, FirstDestBit) switch
        {
            (_, 0) => NoTargetOffsetPrefixCopier.Instance,
            var (a,b) when a == b => EqualSourceTargetOffsetPrefixCopier.Instance,
            var (a,b) when a < b => SourceLessThanTargetOffsetPrefixCopier.Instance,
            _ => TargetLessThanSourceOffsetPrefixCopier.Instance
        };

    public ByteSplicer PrefixSplicer() => new ByteSplicer(FirstDestBit);
    public ByteSplicer PostSplicer() => new(SuffixBits);

    public IBulkByteCopy PostfixCopier() =>
        SuffixBits == 0 ? NullBulkByteCopy.Instance : PostfixCopyOperation.Instance;
}