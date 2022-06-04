using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
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
    private readonly Memory<IBinaryBitmap> result;
    private readonly IHeightClassReaderStrategy heightClassReader;
    private readonly ReadOnlySpan<Segment> referencedSegments;
    
    
    private int bitmapsDecoded = 0;
    private int height = 0;

    public SymbolParser(SymbolDictionaryFlags headerFlags,
        IEncodedReader encodedReader, Memory<IBinaryBitmap> result, IHeightClassReaderStrategy heightClassReader,
        RefinementTemplateSet refinementTemplateSet, ReadOnlySpan<Segment> referencedSegments)
    {
        this.headerFlags = headerFlags;
        EncodedReader = encodedReader;
        RefinementTemplateSet = refinementTemplateSet;
        this.referencedSegments = referencedSegments;
        IndividualBitmapReader = PickReader(headerFlags.AggregateRefinement);
        this.result = result;
        this.heightClassReader = heightClassReader;
    }

    private static IIndividualBitmapReader PickReader(bool useRefinement) => 
        useRefinement ? RefinementBitmapReader.Instance : UnrefinedBitmapReader.Instance;

    public void ReadSymbols(ref SequenceReader<byte> reader)
    {
        do
        {
            ReadHeightClass(ref reader);
        } while (bitmapsDecoded < result.Length);
    }

    private void ReadHeightClass(ref SequenceReader<byte> reader)
    {
        height += EncodedReader.DeltaHeight(ref reader);
        heightClassReader.ReadHeightClassBitmaps(ref reader, ref this, height);
    }
    
    //public methods that serve the HeightClassReaders
    public int BitmapsLeftToDecode() => result.Length - bitmapsDecoded;
    public void AddBitmap(IBinaryBitmap bitmap) => result.Span[bitmapsDecoded++] = bitmap;

    public bool TryGetWidth(ref SequenceReader<byte> source, out int value) =>
        !EncodedReader.IsOutOfBand(value = EncodedReader.DeltaWidth(ref source));

    public IBinaryBitmap ReferencedSymbol(int symbolId) => 
        referencedSegments.GetBitmap(symbolId, result.Span[..bitmapsDecoded]);
}