using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;

public ref struct SymbolParser
{
    private SequenceReader<byte> reader = default;
    private readonly SymbolDictionaryFlags headerFlags;
    public IEncodedReader IntReader { get; }
    private readonly IBinaryBitmap[] result;
    private readonly IHeightClassReaderStrategy heightClassReader;
    private int bitmapsDecoded = 0;
    private int height = 0;

    public SymbolParser(SymbolDictionaryFlags headerFlags,
        IEncodedReader intReader, IBinaryBitmap[] result, IHeightClassReaderStrategy heightClassReader)
    {
        this.headerFlags = headerFlags;
        IntReader = intReader;
        this.result = result;
        this.heightClassReader = heightClassReader;
    }

    public void Parse(ref SequenceReader<byte> reader)
    {
        this.reader = reader;
        Parse();
        reader = this.reader;
    }

    private void Parse()
    {
        if (headerFlags.AggregateRefinement)
            throw new NotImplementedException("Only type 1 dictionary parsing is implemented");
        do
        {
            ReadHeightClass();
        } while (bitmapsDecoded < result.Length);
    }

    private void ReadHeightClass()
    {
        height += IntReader.DeltaHeight(ref reader);
        heightClassReader.ReadHeightClassBitmaps(ref reader, ref this, height);
    }

//    public void AdvancePast(in SequenceReader<byte> source) => reader = source;
    public int BitmapsLeftToDecode() => result.Length - bitmapsDecoded;
    public void AddBitmap(IBinaryBitmap bitmap) => result[bitmapsDecoded++] = bitmap;
}