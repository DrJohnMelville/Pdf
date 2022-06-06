using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.GenericRegionRefinements;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;

public ref struct SymbolParser
{
    private readonly SymbolDictionaryFlags headerFlags;
    public IEncodedReader EncodedReader { get; }
    public RefinementTemplateSet RefinementTemplateSet { get; }
    public IIndividualBitmapReader IndividualBitmapReader { get; }
    public readonly Memory<IBinaryBitmap> SymbolsBeingDecoded;
    private readonly IHeightClassReaderStrategy heightClassReader;
    public ReadOnlySpan<Segment> ReferencedSegments { get; }


    private int bitmapsDecoded = 0;
    private int height = 0;

    public SymbolParser(SymbolDictionaryFlags headerFlags,
        IEncodedReader encodedReader, Memory<IBinaryBitmap> symbolsBeingDecoded, IHeightClassReaderStrategy heightClassReader,
        RefinementTemplateSet refinementTemplateSet, ReadOnlySpan<Segment> referencedSegments)
    {
        this.headerFlags = headerFlags;
        EncodedReader = encodedReader;
        RefinementTemplateSet = refinementTemplateSet;
        this.ReferencedSegments = referencedSegments;
        IndividualBitmapReader = PickReader(headerFlags.AggregateRefinement);
        this.SymbolsBeingDecoded = symbolsBeingDecoded;
        this.heightClassReader = heightClassReader;
    }

    private static IIndividualBitmapReader PickReader(bool useRefinement) => 
        useRefinement ? RefinementBitmapReader.Instance : UnrefinedBitmapReader.Instance;

    public void ReadSymbols(ref SequenceReader<byte> reader)
    {
        do
        {
            ReadHeightClass(ref reader);
        } while (bitmapsDecoded < SymbolsBeingDecoded.Length);
    }

    private void ReadHeightClass(ref SequenceReader<byte> reader)
    {
        height += EncodedReader.DeltaHeight(ref reader);
        heightClassReader.ReadHeightClassBitmaps(ref reader, ref this, height);
    }
    
    //public methods that serve the HeightClassReaders
    public int BitmapsLeftToDecode() => SymbolsBeingDecoded.Length - bitmapsDecoded;
    public void AddBitmap(IBinaryBitmap bitmap) => SymbolsBeingDecoded.Span[bitmapsDecoded++] = bitmap;

    public bool TryGetWidth(ref SequenceReader<byte> source, out int value) =>
        !EncodedReader.IsOutOfBand(value = EncodedReader.DeltaWidth(ref source));

    public IBinaryBitmap ReferencedSymbol(int symbolId) => 
        ReferencedSegments.GetBitmap(symbolId, SymbolsBeingDecoded.Span[..bitmapsDecoded]);
}