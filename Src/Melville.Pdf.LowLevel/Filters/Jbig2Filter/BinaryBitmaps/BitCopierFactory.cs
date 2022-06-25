using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public readonly partial struct BitCopierFactory
{
    [FromConstructor]private readonly int sourceBit;
    [FromConstructor]private readonly int destBit;
    [FromConstructor]private readonly int bitLength;
    [FromConstructor]private readonly CombinationOperator combinationOperator;
    
    public BitCopier Create()
    {
        if (destBit > 0 && destBit + bitLength < 8)
            return SingleByteCopier();


        return MultiByteCopier();
    }

    private BitCopier SingleByteCopier()
    {
        return new BitCopier(
            new OffsetReader(sourceBit),
            new RowCopyPlan(sourceBit, destBit, 0, bitLength, combinationOperator),
            SingleByteCopy.Instance, NullBulkByteCopy.Instance, NullBulkByteCopy.Instance
            );
    }

    private BitCopier MultiByteCopier()
    {
        var (bytes, suffixBits) = Math.DivRem(BitLengthExcludingDestinationPrefix(destBit, bitLength), 8);
        var reader = new OffsetReader(sourceBit);
        var plan = new RowCopyPlan(sourceBit, destBit, bytes, suffixBits, combinationOperator);
        return new BitCopier(reader, plan,
            PrefixCopier(),
            PossiblyTrivialBulkCopier(bytes),
            PostfixCopier(suffixBits));
    }


    private IBulkByteCopy PrefixCopier() =>
        (sourceBit, destBit) switch
        {
            (_, 0) => NoTargetOffsetPrefixCopier.Instance,
            var (a,b) when a == b => EqualSourceTargetOffsetPrefixCopier.Instance,
            var (a,b) when a < b => SourceLessThanTargetOffsetPrefixCopier.Instance,
            _ => TargetLessThanSourceOffsetPrefixCopier.Instance
        };
    
    private IBulkByteCopy PossiblyTrivialBulkCopier(int bytes) => 
        bytes == 0? NullBulkByteCopy.Instance : BulkCopier();

    private IBulkByteCopy BulkCopier() =>
        (sourceBit == destBit, combinationOperator) switch
        {
            (true, CombinationOperator.Replace) => AlignedReplaceBulkCopy.Instance,
            (false, CombinationOperator.Replace) => SourceOffsetBulkCopy.Instance,
            (true, _) => AlignedBulkOperation.Instance,
            _ => SourceOffsetBulkOperation.Instance
        };

    private static IBulkByteCopy PostfixCopier(int suffixBits) =>
        suffixBits == 0 ? NullBulkByteCopy.Instance : PostfixCopyOperation.Instance;


    private static int BitLengthExcludingDestinationPrefix(int destBit, int bitLength) => 
        bitLength - DestinationPrefixLength(destBit);

    private static int DestinationPrefixLength(int destBit) => destBit == 0?0:(8-destBit);
}