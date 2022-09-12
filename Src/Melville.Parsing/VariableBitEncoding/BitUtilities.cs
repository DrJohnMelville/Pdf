namespace Melville.Parsing.VariableBitEncoding;

public static class BitUtilities
{
    public static byte Mask(int bits) => (byte) ((1 << bits) - 1);

    public static (uint HighBits, uint LowBits) SplitHighAndLowBits(this uint i, int lowBitsToKeep) =>
        (i >> lowBitsToKeep, i & Mask(lowBitsToKeep));

    public static uint AddLeastSignificantByte(this uint i, byte b) => (i << 8) | b;

    public static int Exp2(int exp) => 1 << exp;
}