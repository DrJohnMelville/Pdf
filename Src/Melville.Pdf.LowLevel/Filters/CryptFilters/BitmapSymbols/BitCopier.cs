using System;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

public static class BitCopierFactory
{
    public static BitCopier Create(byte sourceBit, byte destBit, uint bitLength, 
        CombinationOperator combinationOperator)
    {
        var (bytes, rightbits) = Math.DivRem(BitLengthExcludingDestinationPrefix(destBit, bitLength), 8);

        var plan = new RowCopyPlan(sourceBit, destBit, (uint)bytes, (byte)rightbits, 
            combinationOperator);

        return new BitCopier(plan);

    }

    private static uint BitLengthExcludingDestinationPrefix(byte destBit, uint bitLength)
    {
        return  (uint) (bitLength - (destBit == 0?0:(8-destBit)));
    }
}

public readonly record struct RowCopyPlan(
    byte FirstSourceBit, byte FirstDestBit,
    uint WholeBytes, byte SuffixBits, CombinationOperator CombinationOperator)
{
    public IBulkByteCopy BulkCopier() =>
        (FirstSourceBit == FirstDestBit, CombinationOperator) switch
        {
            (true, CombinationOperator.Replace) => AlignedReplaceBulkCopy.Instance,
            (false, CombinationOperator.Replace) => SourceOffsetBulkCopy.Instance,
            (true, _) => AlignedBulkOperation.Instance,
            _ => SourceOffsetBulkOperation.Instance
        };

    public IPrefixCopier PrefixCopier() =>
        (FirstSourceBit, FirstDestBit) switch
        {
            (_, 0) => NoTargetOffsetPrefixCopier.Instance,
            var (a,b) when a == b => EqualSourceTargetOffsetPrefixCopier.Instance,
            var (a,b) when a < b => SourceLessThanTargetOffsetPrefixCopier.Instance,
            _ => TargetLessThanSourceOffsetPrefixCopier.Instance
        };

    public ByteSplicer PrefixSplicer() => new ByteSplicer(FirstDestBit);
    public ByteSplicer PostSplicer() => new(SuffixBits);
}

public ref struct BitCopier
{
    public OffsetReader Reader;
    public readonly RowCopyPlan Plan;
    private IPrefixCopier prefixCopier;
    private IBulkByteCopy bulkCopier;
    private ByteSplicer postSplicer;

    public BitCopier(in RowCopyPlan plan)
    {
        Reader = new OffsetReader(plan.FirstSourceBit);
        Plan = plan;
        prefixCopier = plan.PrefixCopier();
        bulkCopier = plan.BulkCopier();
        postSplicer = plan.PostSplicer();
    }

    public unsafe void Copy(in Span<byte> src, in Span<byte> dest)
    {
        fixed(byte* srcPointer = src)
        fixed (byte* destPointer = dest)
        {
            Copy(srcPointer, destPointer);
        }

    }

    public unsafe void Copy(byte* src, byte* dest)
    {
        prefixCopier.CopyPrefix(ref src, ref dest, ref this);
        bulkCopier.Copy(ref src, ref dest, ref this);
        if (Plan.SuffixBits > 0)
        {
            var lastSource = Reader.ReadBye(ref src);
            *dest = postSplicer.SplicePrefixByte(lastSource, *dest, Plan.CombinationOperator);
        }
    }
}


/*

The east case -- source and dest both bytre aligned -- there is no prefix/postfix

source is offset, but destination is byte aligned
+--------------
--+
     Source starts on nth bit of the first byte -- read the prefix and right shift the unused bytes
    then for each byte leftshif
      
*/