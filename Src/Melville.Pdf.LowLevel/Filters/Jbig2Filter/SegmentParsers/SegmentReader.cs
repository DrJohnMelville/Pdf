using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

public static class SegmentReader
{
    public static ValueTask<Segment> ReadFromAsync(this in SegmentHeader header, PipeReader source) =>
        header.SegmentType switch
        {
            SegmentType.EndOfFile => new(Segment.EndOfFile),
            SegmentType.EndOfPage => new(Segment.EndOfPage),
            _ => ReadDataFrom(header, source)
        };

    private static async ValueTask<Segment> ReadDataFrom(SegmentHeader header, PipeReader source)
    {
        var readResult = await source.ReadAtLeastAsync((int)header.DataLength).CA();
        var sequence = readResult.Buffer.Slice(0, header.DataLength);
        var ret = ReadFrom(header, sequence);
        source.AdvanceTo(sequence.End);
        return ret;
    }

    private static Segment ReadFrom(in SegmentHeader header, in ReadOnlySequence<byte> data)
    {
        var reader = new SequenceReader<byte>(data);
        #warning -- need to actually load the referred segments
        ReadOnlySpan<Segment> referencedSegments = ReadOnlySpan<Segment>.Empty;
        return header.SegmentType switch
        {
            SegmentType.SymbolDictionary => new SymbolDictionaryParser(reader, referencedSegments).Parse(),
            SegmentType.EndOfStripe => EndOfStripeSegmentParser.Read(header, ref reader),
            SegmentType.EndOfPage => Segment.EndOfPage,
            SegmentType.EndOfFile => Segment.EndOfFile,
            _ => throw new InvalidDataException("Unknown JBig2 Segment: " + header.SegmentType)
        };
    }
}