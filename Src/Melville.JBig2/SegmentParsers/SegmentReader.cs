using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.SegmentParsers.GenericRegionParsers;
using Melville.JBig2.SegmentParsers.HalftoneRegionParsers;
using Melville.JBig2.SegmentParsers.SymbolDictonaries;
using Melville.JBig2.SegmentParsers.TextRegions;
using Melville.JBig2.Segments;
using Melville.Parsing.IStreamAdaptors;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.JBig2.SegmentParsers;

internal readonly struct SegmentReader
{
    private readonly ParsingBuffer source;
    public readonly SegmentHeader Header;
    private readonly IReadOnlyDictionary<uint, Segment> priorSegments;

    public SegmentReader(ParsingBuffer source, SegmentHeader header, IReadOnlyDictionary<uint, Segment> priorSegments)
    {
        this.source = source;
        this.Header = header;
        this.priorSegments = priorSegments;
    }
    
    public async ValueTask SkipOverAsync()
    {
         await ReadSpanAsync().CA();
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
        var memory = await ReadSpanAsync().CA();
        var ret = ReadFrom(memory.Span);
        return ret;
    }

    private ValueTask<Memory<byte>> ReadSpanAsync() => 
        Header.DataLength == 0xFFFFFFFF ?
            new ReadUnknownSegmentLength(source).ReadSegmentAsync() : 
            ReadKnownLengthSegmentAsync();

    private async ValueTask<Memory<byte>> ReadKnownLengthSegmentAsync()
    {
        var hlen = (int)Header.DataLength;
        return (await source.TakeBytesAsync(hlen));
    }

    private void LookupSegments(Span<Segment> segments)
    {
        Debug.Assert(segments.Length == Header.ReferencedSegmentNumbers.Length);
        for (int i = 0; i < Header.ReferencedSegmentNumbers.Length; i++)
        {
            segments[i] = priorSegments[Header.ReferencedSegmentNumbers[i]];
        }
    }

    private Segment ReadFrom(ReadOnlySpan<byte> data)
    {
        using var rentedSegmentSpan = new PooledBuffer<Segment>(Header.ReferencedSegmentNumbers.Length);
        var referencedSegments = rentedSegmentSpan.AsSpan(0, Header.ReferencedSegmentNumbers.Length);
        LookupSegments(referencedSegments);

        var reader = new SpanParsingBuffer(data);

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

}