using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public readonly struct ByteSplicer
{
    private readonly byte highMask;
    private readonly byte lowMask;

    public ByteSplicer(int highBitsToKeep)
    {
        lowMask = (byte)BottomNBits(8 - highBitsToKeep);
        highMask = (byte)~lowMask;
    }

    private static int BottomNBits(int bits) => (1 << bits) - 1;

    public byte Splice(byte highByte, byte lowByte) =>(byte)((highByte & highMask) | (lowByte & lowMask));

    public byte SplicePrefixByte(byte priorByte, byte newByte, CombinationOperator combinationOperator) =>
        Splice(priorByte, combinationOperator.Combine(priorByte, newByte));
    public byte SplicePostFixByte(byte newByte, byte priorByte, CombinationOperator combinationOperator) =>
        Splice(combinationOperator.Combine(priorByte, newByte), priorByte);
}

