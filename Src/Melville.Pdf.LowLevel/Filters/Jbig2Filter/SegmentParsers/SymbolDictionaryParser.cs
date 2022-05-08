
using System;
using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

public ref struct SymbolDictionaryParser
{
    private SequenceReader<byte> reader;

    public SymbolDictionaryParser(in SequenceReader<byte> reader)
    {
        this.reader = reader;
    }

    public SymbolDictionarySegment Parse(uint number)
    {
        var flags = new SymbolDictionaryFlags(reader.ReadBigEndianUint16());
        if (!flags.UseHuffmanEncoding) 
            throw new NotImplementedException(" Parsing non huffman symbolDictionaries is not supported yet");
        if (flags.AggregateRefinement && !flags.RefAggTeqmplate)
            throw new NotImplementedException("Parsing refinement ATflags is not supported");
        var expotedSymbols = reader.ReadBigEndianUint32();
        var newSymbols = reader.ReadBigEndianUint32();
        return new SymbolDictionarySegment(number, flags, expotedSymbols, newSymbols);
    }
}