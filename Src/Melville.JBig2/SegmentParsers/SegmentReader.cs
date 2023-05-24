using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.SegmentParsers.GenericRegionParsers;
using Melville.JBig2.SegmentParsers.HalftoneRegionParsers;
using Melville.JBig2.SegmentParsers.SymbolDictonaries;
using Melville.JBig2.SegmentParsers.TextRegions;
using Melville.JBig2.Segments;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.JBig2.SegmentParsers;

internal readonly struct SegmentReader
{
    private readonly PipeReader source;
    public readonly SegmentHeader Header;
    private readonly IReadOnlyDictionary<uint, Segment> priorSegments;

    public SegmentReader(PipeReader source, SegmentHeader header, IReadOnlyDictionary<uint, Segment> priorSegments)
    {
        this.source = source;
        this.Header = header;
        this.priorSegments = priorSegments;
    }
    
    public async ValueTask SkipOverAsync()
    {
        var span = await ReadSpanAsync().CA();
        source.AdvanceTo(span.End);
    }
    public ValueTask<Segment> ReadFromAsync() =>
        Header.SegmentType switch
        {
            SegmentType.EndOfFile => new(Segment.EndOfFile),
            SegmentType.EndOfPage => new(Segment.EndOfPage),
            _ => ReadDataFromAsync()
        };

    private  async ValueTask<Segment> ReadDataFromAsync()
    {
        var sequence = await ReadSpanAsync().CA();
        var ret = ReadFrom(sequence);
        source.AdvanceTo(sequence.End);
        return ret;
    }

    private async ValueTask<ReadOnlySequence<byte>> ReadSpanAsync()
    {
        var readResult = await source.ReadAtLeastAsync((int)Header.DataLength).CA();
        var sequence = readResult.Buffer.Slice(0, Header.DataLength);
        return sequence;
    }

    private Segment ReadFrom(in ReadOnlySequence<byte> data)
    {
        var reader = new SequenceReader<byte>(data);
        using var rentedSegmentSpan = new SpanRental<Segment>(Header.ReferencedSegmentNumbers.Length);
        var referencedSegments = rentedSegmentSpan.Span;
        LookupSegments(referencedSegments);
        return Header.SegmentType switch
        {
            SegmentType.SymbolDictionary => new SymbolDictionaryParser(reader, referencedSegments).Parse(),
            SegmentType.PatternDictionary => PatternDictionarySegmentParser.Parse(reader),

            SegmentType.EndOfStripe => EndOfStripeSegmentParser.Read(Header, ref reader),
            SegmentType.EndOfPage => Segment.EndOfPage,
            SegmentType.EndOfFile => Segment.EndOfFile,
            SegmentType.PageInformation => PageInformationSegmentParser.Parse(ref reader),

            SegmentType.ImmediateLosslessTextRegion or SegmentType.ImmediateTextRegion or 
                SegmentType.IntermediateTextRegion=> 
                TextRegionSegmentParser.Parse(reader, referencedSegments),
            SegmentType.ImmediateLosslessGenericRegion or SegmentType.ImmediateGenericRegion
                or SegmentType.IntermediateGenericRegion=> 
                GenericRegionSegmentParser.Parse(reader),
            SegmentType.ImmediateLosslessHalftoneRegion or SegmentType.ImmediateHalftoneRegion
                or SegmentType.IntermediateHalftoneRegion=> 
                HalftoneSegmentParser.Parse(reader, referencedSegments),
            SegmentType.ImmediateGenericRefinementRegion or SegmentType.ImmediateLosslessGenericRefinementRegion
                or SegmentType.IntermediateGenericRefinementRegion =>
                GenericRefinementRegionSegmentParser.Parse(reader, referencedSegments),
            SegmentType.Extension => new ExtensionSegment(SegmentType.Extension),
            _ => throw new InvalidDataException("Unknown JBig2 Segment: " + Header.SegmentType)
        };
    }

    private void LookupSegments(Span<Segment> segments)
    {
        Debug.Assert(segments.Length == Header.ReferencedSegmentNumbers.Length);
        for (int i = 0; i < Header.ReferencedSegmentNumbers.Length; i++)
        {
            segments[i] = priorSegments[Header.ReferencedSegmentNumbers[i]];
        }
    }
}

internal readonly struct SpanRental<T> : IDisposable
{
    private readonly T[] items;
    private readonly int length;
    public readonly Span<T> Span => items.AsSpan(0, length);

    public SpanRental(int length) : this()
    {
        this.length = length;
        items = this.length <= 0? Array.Empty<T>(): ArrayPool<T>.Shared.Rent(length);
    }


    public void Dispose()
    {
        if (length <= 0) return;
        Span.Clear();
        ArrayPool<T>.Shared.Return(items);
    }
}      