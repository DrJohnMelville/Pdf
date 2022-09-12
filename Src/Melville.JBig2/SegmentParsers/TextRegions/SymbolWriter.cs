using System.Buffers;
using Melville.INPC;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.EncodedReaders;
using Melville.JBig2.GenericRegionRefinements;
using Melville.JBig2.Segments;

namespace Melville.JBig2.SegmentParsers.TextRegions;

public ref partial struct SymbolWriter
{
    [FromConstructor] private readonly BinaryBitmapWriter target;
    [FromConstructor] private readonly IEncodedReader integerReader;
    [FromConstructor] private readonly ReadOnlySpan<Segment> characterDictionary;
    [FromConstructor] private readonly ReadOnlySpan<IBinaryBitmap> additionalCharacters;
    [FromConstructor] private int remainingSymbolsToDecode;
    [FromConstructor] private readonly int stripSize;
    [FromConstructor] private readonly int defaultCharacterSpacing;
    [FromConstructor] private readonly bool useRefinement;
    [FromConstructor] private readonly RefinementTemplateSet refinementTemplate;

    // these variables are the current decoding state
    private int strIpT = 0;
    private int firstS = 0;
    private int curS = 0;
    
    public void Decode(ref SequenceReader<byte> source)
    {
        var deltaT = integerReader.DeltaT(ref source) * stripSize;
        strIpT = -deltaT;
        while (remainingSymbolsToDecode > 0) DecodeStrip(ref source);
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
        target.RefineBitsFrom(charT, ref curS, symbol, 
            FloorDiv2(rdh) + rdy, FloorDiv2(rdw) + rdx,
            symbol.Height + rdh, symbol.Width+ rdw, integerReader, refinementTemplate, ref source);
    }

    private int FloorDiv2(int x) =>
        (x < 0 && ((x & 0x1) == 1)) ? (x / 2) - 1 : (x /2 );

    private void CopyUnmodifiedBitmap(int charT, IBinaryBitmap symbol) => 
        target.WriteBitmap(charT, ref curS, symbol);

    private int ReadCharacterDeltaT(ref SequenceReader<byte> source) => 
        stripSize == 1?0: integerReader.TCoordinate(ref source);
}