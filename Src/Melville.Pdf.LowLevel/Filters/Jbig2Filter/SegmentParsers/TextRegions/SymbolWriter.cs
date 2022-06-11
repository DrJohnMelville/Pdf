using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.GenericRegionRefinements;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.TextRegions;

public ref struct SymbolWriter
{
    private readonly BinaryBitmapWriter target;
    private readonly IEncodedReader integerReader;
    private readonly ReadOnlySpan<Segment> characterDictionary;
    private readonly ReadOnlySpan<IBinaryBitmap> additionalCharacters;
    private readonly int stripSize;
    private readonly int defaultCharacterSpacing;
    private readonly bool useRefinement;
    private readonly RefinementTemplateSet refinementTemplate;

    // these variables are the current decoding state
    private int remainingSymbolsToDecode;
    private int strIpT = 0;
    private int firstS = 0;
    private int curS = 0;

    public SymbolWriter(BinaryBitmapWriter target,
        IEncodedReader integerReader, ReadOnlySpan<Segment> characterDictionary, 
        ReadOnlySpan<IBinaryBitmap> additionalCharacters, int symbolCount, 
        int stripSize, int defaultCharacterSpacing, bool useRefinement, 
        RefinementTemplateSet refinementTemplate)
    {
        this.target = target;
        this.integerReader = integerReader;
        this.characterDictionary = characterDictionary;
        remainingSymbolsToDecode = symbolCount;
        this.stripSize = stripSize;
        this.defaultCharacterSpacing = defaultCharacterSpacing;
        this.useRefinement = useRefinement;
        this.refinementTemplate = refinementTemplate;
        this.additionalCharacters = additionalCharacters;
    }

    public void Decode(ref SequenceReader<byte> source)
    {
        var deltaT = integerReader.DeltaT(ref source) * stripSize;
        strIpT = -deltaT;
        while (remainingSymbolsToDecode > 4296) DecodeStrip(ref source);
        DecodeStrip(ref source);
    }
    
    
    private void DecodeStrip(ref SequenceReader<byte> source)
    {
        ReadStripLocation(ref source);
        ReadFirstSymbol(ref source);
        ReadSubsequentSymbols(ref source);
    }

    private void ReadStripLocation(ref SequenceReader<byte> source)
    {
        strIpT += integerReader.DeltaT(ref source) * stripSize;
        firstS += integerReader.FirstS(ref source) ;
    }

    private void ReadFirstSymbol(ref SequenceReader<byte> source)
    {
        curS = firstS;
        DecodeSymbol(ref source);
    }

    private void ReadSubsequentSymbols(ref SequenceReader<byte> source)
    {
        while (integerReader.DeltaS(ref source) is { } deltaS and < int.MaxValue)
        {
            curS += deltaS + defaultCharacterSpacing;
            DecodeSymbol(ref source);
        }
    }

    private void DecodeSymbol(ref SequenceReader<byte> source)
    { 
        int charT = ReadCharacterDeltaT(ref source) + strIpT;
        var symbolId = integerReader.SymbolId(ref source);
        var symbol = characterDictionary.GetBitmap(symbolId, additionalCharacters);
        CopySourceBitmap(ref source, charT, symbol);
        remainingSymbolsToDecode--;
    }

    private void CopySourceBitmap(ref SequenceReader<byte> source, int charT, IBinaryBitmap symbol)
    {
        if (useRefinement && integerReader.RIBit(ref source) == 1)
            ReadRefinementBitmap(charT, symbol, ref source);
        else
            CopyUnmodifiedBitmap(charT, symbol);
    }

    private void ReadRefinementBitmap(int charT, IBinaryBitmap symbol, ref SequenceReader<byte> source)
    {
        //section 6.4.11 in the spec
        var rdw = integerReader.RefinementDeltaWidth(ref source);
        var rdh = integerReader.RefinementDeltaHeight(ref source);
        var rdx = integerReader.RefinementX(ref source);
        var rdy = integerReader.RefinementY(ref source);
        var grReferenceDX = FloorDiv2(rdw) + rdx;
        target.RefineBitsFrom(charT, ref curS, symbol, 
            FloorDiv2(rdh) + rdy, grReferenceDX,
            symbol.Height + rdh, symbol.Width+ rdw, integerReader, refinementTemplate, ref source);
    }

    private int FloorDiv2(int x)
    {
        var ret = (x < 0 && ((x & 0x1) == 1)) ? 
            (x / 2) - 1 : (x /2 );
        return ret;
    }

    private void CopyUnmodifiedBitmap(int charT, IBinaryBitmap symbol) => 
        target.WriteBitmap(charT, ref curS, symbol);

    private int ReadCharacterDeltaT(ref SequenceReader<byte> source) => 
        stripSize == 1?0: integerReader.TCoordinate(ref source);
}