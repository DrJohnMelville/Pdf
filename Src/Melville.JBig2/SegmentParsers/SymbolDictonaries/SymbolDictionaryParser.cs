using System;
using System.Buffers;
using Melville.JBig2.ArithmeticEncodings;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.EncodedReaders;
using Melville.JBig2.GenericRegionRefinements;
using Melville.JBig2.HuffmanTables;
using Melville.JBig2.Segments;
using Melville.Parsing.SequenceReaders;

namespace Melville.JBig2.SegmentParsers.SymbolDictonaries;

internal ref struct SymbolDictionaryParser
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
        //these method may read from the bitstream, so their order is essential
        var intReader = flags.UseHuffmanEncoding ? HuffmanIntReader() : ParseAtDecoder();
        var refinementReader = ParseRefinementInfo();
        var exportParser = CreateSymbolDictionaryParser();
        if (flags.AggregateRefinement)
            intReader.PrepareForRefinementSymbolDictionary((uint)
                referencedSegments.CountSourceBitmaps() + exportParser.NewSymbols);

        if (flags.BitmapContextUsed)
            throw new NotImplementedException("Does not yet preserve bitmap context");

        var imgBuffer = ArrayPool<IBinaryBitmap>.Shared.Rent((int)exportParser.NewSymbols);
        new SymbolParser(flags, intReader, imgBuffer.AsMemory(0,(int)exportParser.NewSymbols), 
            SelectHeightClassStrategy(), refinementReader, referencedSegments).ReadSymbols(ref reader);
        var ret = new SymbolDictionarySegment(exportParser.ParseExportedArray(ref reader, intReader,
            imgBuffer, referencedSegments));
        ArrayPool<IBinaryBitmap>.Shared.Return(imgBuffer);
        return ret;
    }

    private RefinementTemplateSet ParseRefinementInfo() => !flags.AggregateRefinement ? 
        new RefinementTemplateSet() : // returns an empty object, which never gets touched 
        new RefinementTemplateSet(ref reader, flags.RefAggTeqmplate);


    private IHeightClassReaderStrategy SelectHeightClassStrategy() => 
        ShouldUseCompositeBitmaps()? CompositeHeightClassReader.Instance: 
            IndividualHeightClassReader.Instance;

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

    private SymbolDictionaryExportParser CreateSymbolDictionaryParser()
    {
        var exportedSymbols = reader.ReadBigEndianUint32();
        var newSymbols = reader.ReadBigEndianUint32();
        return new SymbolDictionaryExportParser(exportedSymbols, newSymbols);
    }
}