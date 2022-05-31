using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;

public ref struct SymbolParser
{
    private SequenceReader<byte> reader = default;
    private readonly SymbolDictionaryFlags headerFlags;
    public readonly IIntegerDecoder HeightReader;
    public readonly IIntegerDecoder WidthReader;
    public readonly IIntegerDecoder SizeReader;
    private readonly IBinaryBitmap[] result;
    private readonly IHeightClassReaderStrategy heightClassReader;
    private int bitmapsDecoded = 0;
    private int height = 0;

    public SymbolParser(SymbolDictionaryFlags headerFlags,
        IIntegerDecoder heightReader, IIntegerDecoder widthReader, IIntegerDecoder sizeReader, 
        IBinaryBitmap[] result, IHeightClassReaderStrategy heightClassReader)
    {
        Debug.Assert(widthReader.HasOutOfBandRow());
        Debug.Assert(!heightReader.HasOutOfBandRow() || !headerFlags.UseHuffmanEncoding);
        Debug.Assert(!sizeReader.HasOutOfBandRow() || !headerFlags.UseHuffmanEncoding);
        this.headerFlags = headerFlags;
        HeightReader = heightReader;
        WidthReader = widthReader;
        SizeReader = sizeReader;
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
        var source = new BitSource(reader);
        height += HeightReader.GetInteger(ref source);
        heightClassReader.ReadHeightClassBitmaps(ref source, ref this, height);
    }

    public void AdvancePast(in SequenceReader<byte> source) => reader = source;
    public int BitmapsLeftToDecode() => result.Length - bitmapsDecoded;
    public void AddBitmap(IBinaryBitmap bitmap) => result[bitmapsDecoded++] = bitmap;
}