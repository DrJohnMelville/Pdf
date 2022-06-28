using System;
using System.Buffers;
using System.Diagnostics;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

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

    public HuffmanCode AddBitToPattern(ref SequenceReader<byte> reader, BitReader bitState) => 
        new(PrefixLength + 1, (prefixData << 1) | bitState.ForceRead(1, ref reader));

    public bool IsEqualTo(in HuffmanCode other) => PrefixLength == other.PrefixLength && prefixData == other.prefixData;
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

    public bool Matches(in HuffmanCode other)
    {
        return code.IsEqualTo(other);
    }

    public int ReadNum(ref SequenceReader<byte> source, BitReader bitState) =>
        AdjustNumber(bitState.ForceRead(rangeLength, ref source));
    private int AdjustNumber(int raw) => rangeOffset + (rangeFactor * raw);
    public bool IsOutOfBandRow => rangeLength == 0 && rangeOffset == int.MaxValue;
}

public static class HuffmanDebugSupport
{
    public static HuffmanLine[] VerifyHasOutOfBand(this HuffmanLine[] lines)
    {
        Debug.Assert(HasOutOfBand(lines));
        return lines;
    }
    public static HuffmanLine[] VerifyNoOutOfBand(this HuffmanLine[] lines)
    {
        Debug.Assert(!HasOutOfBand(lines));
        return lines;
    }
    private static bool HasOutOfBand(this HuffmanLine[] lines) => lines.Any(i=>i.IsOutOfBandRow);
}