using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.GenericRegionRefinements;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        while (remainingSymbolsToDecode > 0) DecodeStrip(ref source);
    }

    private void DecodeStrip(ref SequenceReader<byte> source)
    {
        strIpT += integerReader.DeltaT(ref source) * stripSize;

        firstS += integerReader.FirstS(ref source);
        curS = firstS;
        DecodeSymbol(ref source);
        while (integerReader.DeltaS(ref source) is { } deltaS and < int.MaxValue)
        {
            curS += deltaS + defaultCharacterSpacing;
            #warning see page 27 3) c) ii -- it think I need an SBDSOFFSET in the line above
            DecodeSymbol(ref source);
        }
        // decode subsequent symbols
    }

    private void DecodeSymbol(ref SequenceReader<byte> source)
    {
        int charT = ReadCharacterDeltaT(ref source) + strIpT;
        var symbolId = integerReader.SymbolId(ref source);
        var symbol = characterDictionary.GetBitmap(symbolId, additionalCharacters);
        if (useRefinement)  
            throw new NotImplementedException("Need to refine the symbol Bitmap");
        CopyUnmodifiedBitmap(charT, symbol);
        remainingSymbolsToDecode--;
    }

    private void CopyUnmodifiedBitmap(int charT, IBinaryBitmap symbol) => target.WriteBitmap(charT, ref curS, symbol);

    private int ReadCharacterDeltaT(ref SequenceReader<byte> source) => 
        stripSize == 1?0: integerReader.TCoordinate(ref source);
}