using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.TextRegions;

public ref struct SymbolWriter
{
    private readonly BinaryBitmapWriter target;

    /// <summary>
    /// SBHUFFFS
    /// </summary>
    private readonly TextRegionFlags regionFlags;

    private readonly IEncodedReader integerReader;
    private readonly ReadOnlySpan<Segment> characterDictionary;

    // these variables are the current decoding state
    private int remainingSymbolsToDecode;
    private int strIpT = 0;
    private int firstS = 0;
    private int curS = 0;

    public SymbolWriter(BinaryBitmapWriter target, TextRegionFlags regionFlags,
        IEncodedReader integerReader, ReadOnlySpan<Segment> characterDictionary, int symbolCount)
    {
        this.target = target;
        this.regionFlags = regionFlags;
        this.integerReader = integerReader;
        this.characterDictionary = characterDictionary;
        remainingSymbolsToDecode = symbolCount;
    }

    public void Decode(ref SequenceReader<byte> source)
    {
        var deltaT = integerReader.DeltaT(ref source) * regionFlags.StripSize;
        strIpT = -deltaT;
        while (remainingSymbolsToDecode > 0) DecodeStrip(ref source);
    }

    private void DecodeStrip(ref SequenceReader<byte> source)
    {
        strIpT += integerReader.DeltaT(ref source) * regionFlags.StripSize;

        firstS += integerReader.FirstS(ref source);
        curS = firstS;
        DecodeSymbol(ref source);
        while (integerReader.DeltaS(ref source) is { } deltaS and < int.MaxValue)
        {
            curS += deltaS;
            DecodeSymbol(ref source);
        }
        // decode subsequent symbols
    }

    private void DecodeSymbol(ref SequenceReader<byte> source)
    {
        var charT = integerReader.TCoordinate(ref source) + strIpT;
        var symbolId = integerReader.SymbolId(ref source);
        var symbol = characterDictionary.GetBitmap(symbolId);
        target.WriteBitmap(charT, ref curS, symbol);
        curS += regionFlags.DefaultCharacteSpacing;
        remainingSymbolsToDecode--;
    }
}