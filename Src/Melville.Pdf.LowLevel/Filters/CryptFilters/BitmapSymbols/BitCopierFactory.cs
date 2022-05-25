using System;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

public static class BitCopierFactory
{
    public static BitCopier Create(byte sourceBit, byte destBit, uint bitLength, 
        CombinationOperator combinationOperator)
    {
        var (bytes, rightbits) = Math.DivRem(BitLengthExcludingDestinationPrefix(destBit, bitLength), 8);
        var plan = new RowCopyPlan(sourceBit, destBit, bytes, (byte)rightbits, combinationOperator);
        return new BitCopier(plan);

    }

    private static uint BitLengthExcludingDestinationPrefix(byte destBit, uint bitLength) => 
        (uint) (bitLength - DestinationPrefixLength(destBit));

    private static int DestinationPrefixLength(byte destBit) => destBit == 0?0:(8-destBit);
}