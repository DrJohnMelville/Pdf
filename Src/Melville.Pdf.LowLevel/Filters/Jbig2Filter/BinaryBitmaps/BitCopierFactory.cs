using System;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public static class BitCopierFactory
{
    public static BitCopier Create(byte sourceBit, byte destBit, int bitLength, 
        CombinationOperator combinationOperator)
    {
        if (destBit > 0 && destBit + bitLength < 8) throw new NotImplementedException("short write algorithm");
        var (bytes, rightbits) = Math.DivRem(BitLengthExcludingDestinationPrefix(destBit, bitLength), 8);
        var plan = new RowCopyPlan(sourceBit, destBit, bytes, (byte)rightbits, combinationOperator);
        return new BitCopier(plan);

    }

    private static int BitLengthExcludingDestinationPrefix(byte destBit, int bitLength) => 
        bitLength - DestinationPrefixLength(destBit);

    private static int DestinationPrefixLength(byte destBit) => destBit == 0?0:(8-destBit);
}