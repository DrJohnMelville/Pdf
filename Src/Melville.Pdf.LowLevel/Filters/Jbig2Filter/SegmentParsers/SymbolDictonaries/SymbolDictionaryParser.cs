using System;
using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
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
        
        var (intReader, symbols) = ParseBlockHeader();

        new SymbolParser(flags, intReader, symbols, SelectHeightClassStrategy()).ReadSymbols(ref reader);
        
        return new SymbolDictionarySegment(symbols, ReadExportedSymbols(symbols, intReader));
    }

    private (IEncodedReader intReader, IBinaryBitmap[] symbols) ParseBlockHeader()
    {
        //these method may read from the bitstream, so their order is essential
        var intReader = flags.UseHuffmanEncoding ? HuffmanIntReader() : ParseAtDecoder();
        var symbols = CreateSymbolArray();
        return (intReader, symbols);
    }

    private IHeightClassReaderStrategy SelectHeightClassStrategy() => 
        ShouldUseCompositeBitmaps()? CompositeHeightClassReader.Instance: IndividualHeightClassReader.Instance;

    private bool ShouldUseCompositeBitmaps() => flags.UseHuffmanEncoding && !flags.AggregateRefinement;


    private IEncodedReader HuffmanIntReader() => new HuffmanIntegerDecoder()
        {
            ExportFlagsContext = StandardHuffmanTables.B1,
            DeltaHeightContext = GetHuffmanTable(flags.HuffmanSelectionForHeight).VerifyNoOutOfBand(),
            DeltaWidthContext = GetHuffmanTable(flags.HuffmanSelectionForWidth).VerifyHasOutOfBand(),
            BitmapSizeContext = GetHuffmanTable(flags.HuffmanSelectionBitmapSize).VerifyNoOutOfBand(),
            AggregationSymbolInstancesContext = GetHuffmanTable(flags.HuffmanTableSelectionAggInst).VerifyNoOutOfBand()
        };
    private HuffmanLine[] GetHuffmanTable(HuffmanTableSelection selection) =>
        selection.GetTableLines(ref referencedSegments);
    
    private  ArithmeticIntegerDecoder ParseAtDecoder() => 
        new(
            new ArithmeticBitmapReaderContext(
            BitmapTemplateFactory.ReadContext(ref reader, flags.GenericRegionTemplate)))
        {
            ExportFlagsContext = new ContextStateDict(9),
            DeltaHeightContext = new ContextStateDict(9),
            DeltaWidthContext = new ContextStateDict(9),
            BitmapSizeContext= new ContextStateDict(9),
            AggregationSymbolInstancesContext = new ContextStateDict(9),
        };

    private Memory<IBinaryBitmap> ReadExportedSymbols(IBinaryBitmap[] symbols, IEncodedReader intReader)
    {
        var offset = intReader.ExportFlags(ref reader);
        var length = intReader.ExportFlags(ref reader);
        if (offset > 0 || length != symbols.Length)
            throw new NotImplementedException(
                "right now this code assumes that the dictionary imports no other dictionaries, and exports all its symbols");
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
    
}