namespace Melville.JBig2.Segments;

internal static class BitOperations
{
    public static bool CheckBit(this int source, int bit) => (source & bit) == bit;
    public static int SetBit(this int source, int bit) => source | bit;
    public static int ClearBit(this int source, int bit) => source & (~bit);

    public static int UnsignedInteger(this int source, int offset, int mask)
        => ((source >> offset) & mask);

    public static int AsSignedInteger(int data, int bits) =>
        CheckBit(data, 1 << (bits - 1)) ? -1 * InvertSignedNumber(data, bits) : data;
    private static int InvertSignedNumber(int data, int bits) => TwosComplement(data) & MaskForBits(bits);
    private static int TwosComplement(int data) => ((~data) + 1);
    public static int MaskForBits(int bits) => (1 << bits) - 1;
}