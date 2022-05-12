namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

public readonly struct HuffmanLine
{
    public int PrefixLengh { get; }

    private readonly int prefixData;
    private readonly int rangeLength;
    private readonly int rangeOffset;
    private readonly int rangeFactor;

    public HuffmanLine(int prefixLengh, int prefixData, int rangeLength, int rangeOffset, int rangeFactor)
    {
        this.rangeLength = rangeLength;
        this.rangeOffset = rangeOffset;
        this.rangeFactor = rangeFactor;
        PrefixLengh = prefixLengh;
        this.prefixData = prefixData;
    }

    public bool Matches(int patternLen, int pattern) => patternLen == PrefixLengh && pattern == prefixData;
    public int ReadNum(ref BitSource source) => rangeOffset + (rangeFactor * source.ReadInt(rangeLength));
    public bool IsOutOfBandRow => rangeLength == 0 && rangeOffset == int.MaxValue;
}