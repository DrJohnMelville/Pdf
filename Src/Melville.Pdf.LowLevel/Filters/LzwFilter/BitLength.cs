namespace Melville.Pdf.LowLevel.Filters.LzwFilter;

internal class BitLength
{
    public int Length { get; private set; }
    private int nextIncrement;
    private int sizeSwitchFlavorDelta = 1;

    public BitLength(int bits, int sizeSwitchFlavorDelta)
    {
        this.sizeSwitchFlavorDelta = sizeSwitchFlavorDelta;
        SetBitLength(bits);
    }

    public void SetBitLength(int bits)
    {
        Length = bits;
        nextIncrement = AlreadyAtMaximumBitLength() ? 
            int.MaxValue:CurrentBitLengthExhaustedAt(bits);
    }

    private bool AlreadyAtMaximumBitLength() => 
        Length >= LzwConstants.MaxBitLength;

    private int CurrentBitLengthExhaustedAt(int bits) => 
        SmallestNumberToBigForBits(bits) - sizeSwitchFlavorDelta;

    private static int SmallestNumberToBigForBits(int bits) => (1 << bits);

    public void CheckBitLength(int next)
    {
        if (next < nextIncrement) return;
        SetBitLength(Length + 1);
    }
}