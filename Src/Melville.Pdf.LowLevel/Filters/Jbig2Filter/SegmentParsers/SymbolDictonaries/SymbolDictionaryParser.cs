using System;
using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;

public ref struct SymbolDictionaryParser
{
    private SequenceReader<byte> reader;
    private ReadOnlySpan<Segment> referencedSegments;

    public SymbolDictionaryParser(in SequenceReader<byte> reader, in ReadOnlySpan<Segment> referencedSegments)
    {
        this.reader = reader;
        this.referencedSegments = referencedSegments;
    }

    public SymbolDictionarySegment Parse(uint number)
    {
        var flags = new SymbolDictionaryFlags(reader.ReadBigEndianUint16());
        CheckTemporaryAssumptions(flags);
        var symbols = CreateSymbolArray();

        var heightHuffman = GetHuffmanTable(flags.HuffmanSelectionForHeight);
        var widthHuffman = GetHuffmanTable(flags.HuffmanSelectionForWidth);
        var bitmapSizeHuffman = GetHuffmanTable(flags.HuffmanSelectionBitmapSize);
        var aggregationHuffman = GetHuffmanTable(flags.HuffmanTableSelectionAggInst);
        Debug.Assert(!aggregationHuffman.HasOutOfBandRow());

        new SymbolParser(flags, heightHuffman, widthHuffman, bitmapSizeHuffman, symbols).Parse(ref reader);
        
        
        return new SymbolDictionarySegment(symbols, ReadExportedSymbols(symbols));
    }

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
        if (!flags.UseHuffmanEncoding)
            throw new NotImplementedException(" Parsing non huffman symbolDictionaries is not supported yet");
        if (flags.AggregateRefinement && !flags.RefAggTeqmplate)
            throw new NotImplementedException("Parsing refinement ATflags is not supported");
    }

    private HuffmanTable GetHuffmanTable(HuffmanTableSelection selection) => 
        selection.GetTable(ref referencedSegments);
}