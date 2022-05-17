namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

public readonly struct HuffmanCode
{
    public int PrefixLength { get; }
    private readonly int prefixData;

    public HuffmanCode(int prefixLength, int prefixData)
    {
        this.prefixData = prefixData;
        PrefixLength = prefixLength;
    }
    public bool Matches(int patternLen, int pattern) => patternLen == PrefixLength && pattern == prefixData;
}

public readonly struct HuffmanLine
{
    public int PrefixLengh => code.PrefixLength;
    private readonly HuffmanCode code;
    private readonly int rangeLength;
    private readonly int rangeOffset;
    private readonly int rangeFactor;

    public HuffmanLine(int prefixLengh, int prefixData, int rangeLength, int rangeOffset, int rangeFactor)
    {
        code = new HuffmanCode(prefixLengh, prefixData);
        this.rangeLength = rangeLength;
        this.rangeOffset = rangeOffset;
        this.rangeFactor = rangeFactor;
    }

    public bool Matches(int patternLen, int pattern) => code.Matches(patternLen, pattern);
    public int ReadNum(ref BitSource source) => rangeOffset + (rangeFactor * source.ReadInt(rangeLength));
    public bool IsOutOfBandRow => rangeLength == 0 && rangeOffset == int.MaxValue;
}