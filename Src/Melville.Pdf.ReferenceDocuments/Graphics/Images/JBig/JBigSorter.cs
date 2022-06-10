using System.Buffers;
using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.FileOrganization;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.JBig;

public class JBigSorter
{
    private readonly byte[] sourceBuffer;
    private readonly MemoryStream globals;
    private readonly MemoryStream specific;
    private readonly int desiredPage;
    private int pos = 9;
    
    public JBigSorter(byte[] source, MemoryStream globals, MemoryStream specific, int desiredPage)
    {
        sourceBuffer = source;
        this.globals = globals;
        this.specific = specific;
        this.desiredPage = desiredPage;
    }

    public void Sort()
    {
        var fileFlags = new FileFlags(sourceBuffer[8]);
        SkipPageCount(fileFlags);
        if (fileFlags.SequentialFileOrganization)
            ParseSequential(); // empty loop is intentional
        else ParseRandom();
    }

    private void ParseRandom()
    {
        while (TryEnqueueOneHeader()) ; // empty loop
        while (segments.Count > 0)
            WriteSegment();
    }

    public record struct SegmentRecord(int Position, int Length, SegmentHeader Header);

    private readonly Queue<SegmentRecord> segments = new();

    private void ParseSequential()
    {
        while (true)
        {
            if (!TryEnqueueOneHeader()) return;
            WriteSegment();
        }
    }

    private bool TryEnqueueOneHeader() => TryEnqueueOneHeader(ParseOneHeader());

    private SegmentRecord ParseOneHeader()
    {
        var seq = new ReadOnlySequence<byte>(sourceBuffer.AsMemory(pos));
        var reader = new SequenceReader<byte>(seq);
        var startingPos = reader.Position;
        SegmentHeaderParser.TryParse(ref reader, out var header);
        var length = (int)seq.Slice(startingPos, reader.Position).Length;
        var segmentRecord = new SegmentRecord(pos, length, header);
        pos += length;
        return segmentRecord;
    }

    private bool TryEnqueueOneHeader(in SegmentRecord segmentRecord)
    {
        segments.Enqueue(segmentRecord);
        return segmentRecord.Header.SegmentType != SegmentType.EndOfFile;
    }

    public void WriteSegment()
    {
        var header = segments.Dequeue();
        if (WantPage(header.Header.Page) && ! SegmentIsUnusedInPdf(header.Header))
            WriteSegment(header.Header, sourceBuffer.AsSpan(header.Position, header.Length), 
                sourceBuffer.AsSpan(pos, (int)header.Header.DataLength));
        pos += (int) header.Header.DataLength;
    }


    private void SkipPageCount(FileFlags fileFlags)
    {
        if (!fileFlags.UnknownPageCount) pos += 4;
    }

    private static bool SegmentIsUnusedInPdf(SegmentHeader readerHeader) => 
        readerHeader.SegmentType is SegmentType.EndOfFile or SegmentType.EndOfPage;

    private bool WantPage(uint headerPage) => headerPage == 0 || headerPage == desiredPage;

    private void WriteSegment(SegmentHeader header, Span<byte> headerSpan, Span<byte> dataSpan)
    {
        var target = header.Page == 0 ? globals : specific;
        if (header.Page is not 0 or 1)
        {
            FixHeaderPage(header, headerSpan);
        }
        target.Write(headerSpan);
        target.Write(dataSpan);
    }

    private void FixHeaderPage(SegmentHeader header, Span<byte> headerSpan)
    {
        headerSpan[^5] = 1;
    }
}