using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;

public ref struct SymbolParser
{
    private readonly SymbolDictionaryFlags headerFlags;
    public IEncodedReader EncodedReader { get; }
    private readonly IBinaryBitmap[] result;
    private readonly IHeightClassReaderStrategy heightClassReader;
    private int bitmapsDecoded = 0;
    private int height = 0;

    public SymbolParser(SymbolDictionaryFlags headerFlags,
        IEncodedReader encodedReader, IBinaryBitmap[] result, IHeightClassReaderStrategy heightClassReader)
    {
        this.headerFlags = headerFlags;
        EncodedReader = encodedReader;
        this.result = result;
        this.heightClassReader = heightClassReader;
    }

    public void ReadSymbols(ref SequenceReader<byte> reader)
    {
        if (headerFlags.AggregateRefinement)
            throw new NotImplementedException("Only type 1 dictionary parsing is implemented");
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
    public void AddBitmap(IBinaryBitmap bitmap) => result[bitmapsDecoded++] = bitmap;

    public bool TryGetWidth(ref SequenceReader<byte> source, out int value) =>
        !EncodedReader.IsOutOfBand(value = EncodedReader.DeltaWidth(ref source));
}