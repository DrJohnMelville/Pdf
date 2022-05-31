using System;
using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;

public ref struct SymbolDictionaryParser
{
    private SequenceReader<byte> reader;
    private ReadOnlySpan<Segment> referencedSegments;
    private readonly SymbolDictionaryFlags flags;

    public SymbolDictionaryParser(in SequenceReader<byte> reader, in ReadOnlySpan<Segment> referencedSegments)
    {
        this.reader = reader;
        this.referencedSegments = referencedSegments;
        flags = new SymbolDictionaryFlags(this.reader.ReadBigEndianUint16());
    }

    public SymbolDictionarySegment Parse()
    {
        CheckTemporaryAssumptions(flags);
        var heightReader = flags.UseHuffmanEncoding?
            CompositeHeightClassReaderStrategy.Instance:
            ParseAtDecoder();
        var symbols = CreateSymbolArray();

        var heightHuffman = GetHuffmanTable(flags.HuffmanSelectionForHeight);
        var widthHuffman = GetHuffmanTable(flags.HuffmanSelectionForWidth);
        var bitmapSizeHuffman = GetHuffmanTable(flags.HuffmanSelectionBitmapSize);
        var aggregationHuffman = GetHuffmanTable(flags.HuffmanTableSelectionAggInst);
        Debug.Assert(!aggregationHuffman.HasOutOfBandRow());

        new SymbolParser(flags, heightHuffman, widthHuffman, bitmapSizeHuffman, symbols,
            heightReader).Parse(ref reader);
        
        return new SymbolDictionarySegment(symbols, ReadExportedSymbols(symbols));
    }

    private IHeightClassReaderStrategy ParseAtDecoder() => 
        new ArithmeticHeightClassReader(
            BitmapTemplateFactory.ReadContext(ref reader, flags.SymbolDictionaryTemplate));

    private Memory<IBinaryBitmap> ReadExportedSymbols(IBinaryBitmap[] symbols)
    {
        var bits = new BitSource(reader);
        var offset = StandardHuffmanTables.B1.GetInteger(ref bits);
        var length = StandardHuffmanTables.B1.GetInteger(ref bits);
        return symbols.AsMemory(offset, length);
    }

    private IBinaryBitmap[] CreateSymbolArray()
    {
        var exportedSymbols = reader.ReadBigEndianUint32();
        var newSymbols = reader.ReadBigEndianUint32();
        if (exportedSymbols != newSymbols)
            throw new NotImplementedException("must export all symbols");
        var symbols = new IBinaryBitmap[newSymbols];
        return symbols;
    }

    private static void CheckTemporaryAssumptions(SymbolDictionaryFlags flags)
    {
        if (flags.AggregateRefinement)
            throw new NotImplementedException("Aggregate refinement is not implemented yet");
        if (flags.AggregateRefinement && !flags.RefAggTeqmplate)
            throw new NotImplementedException("Parsing refinement ATflags is not supported");
    }

    private IIntegerDecoder GetHuffmanTable(HuffmanTableSelection selection) => 
         flags.UseHuffmanEncoding?
             selection.GetTable(ref referencedSegments):
             new ArithmeticIntegerDecoder();
}