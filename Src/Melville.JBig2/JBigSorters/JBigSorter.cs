using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.JBig2.FileOrganization;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Memory;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.JBig2.JBigSorters;

/// <summary>
/// Given a JBig file sort it into the global and specific segments needed to render a given page.
///
/// This is used to generate some of the JBig tests
/// </summary>
public class JBigSorter
{
    private readonly byte[] sourceBuffer;
    private readonly ParsingBuffer parsingBuffer;
    private readonly MemoryStream globals;
    private readonly MemoryStream specific;
    private readonly int desiredPage;
    
    /// <summary>
    /// Sort a JBig into global and specific segments
    /// </summary>
    /// <param name="source">The source JBIG</param>
    /// <param name="globals">Stream that receives the global segments</param>
    /// <param name="specific">Stream that receives the specific segments</param>
    /// <param name="desiredPage">The page to render/</param>
    public JBigSorter(byte[] source, MemoryStream globals, MemoryStream specific, int desiredPage)
    {
        sourceBuffer = source;
        this.globals = globals;
        this.specific = specific;
        this.desiredPage = desiredPage;
        parsingBuffer = new ParsingBuffer(new MemoryReader(source));
    }

    /// <summary>
    /// Parse the source buffer and write out thee sorted segments to their respective buffers
    /// </summary>
    public async ValueTask SortAsync()
    {
        await parsingBuffer.AdvanceAsync(9);
        var fileFlags = new FileFlags(sourceBuffer[8]);
        await SkipPageCountAsync(fileFlags).CA();
        if (fileFlags.SequentialFileOrganization)
            await ParseSequentialAsync(); 
        else await ParseRandomAsync();
        parsingBuffer.Dispose();
    }

    private async Task ParseRandomAsync()
    {
        SegmentRecord sr;
        do
        {
            sr = await ParseOneHeaderAsync().CA();
        } while (TryEnqueueOneHeader(sr));
        while (segments.Count > 0)
            await WriteSegmentAsync().CA();
    }

    private record struct SegmentRecord(int Position, int Length, SegmentHeader Header);

    private readonly Queue<SegmentRecord> segments = new();

    private async ValueTask ParseSequentialAsync()
    {
        while (true)
        {
            var header = await ParseOneHeaderAsync().CA();
            if (!TryEnqueueOneHeader(header)) return;
            await WriteSegmentAsync();
        }
    }


    private async ValueTask<SegmentRecord> ParseOneHeaderAsync()
    {
        var startingPos = parsingBuffer.RelativePosition;
        var header = await SegmentHeaderParser.ParseAsync(parsingBuffer).CA();
        var length = parsingBuffer.RelativePosition - startingPos;
        var segmentRecord = new SegmentRecord((int)startingPos, (int)length, header);
        return segmentRecord;
    }

    private bool TryEnqueueOneHeader(in SegmentRecord segmentRecord)
    {
        segments.Enqueue(segmentRecord);
        return segmentRecord.Header.SegmentType != SegmentType.EndOfFile;
    }

    private ValueTask WriteSegmentAsync()
    {
        var header = segments.Dequeue();
        if (WantPage(header.Header.Page) && ! SegmentIsUnusedInPdf(header.Header))
            WriteSegment(header.Header, sourceBuffer.AsSpan(header.Position, header.Length), 
                sourceBuffer.AsSpan((int)parsingBuffer.RelativePosition, (int)header.Header.DataLength));
        return parsingBuffer.AdvanceAsync((int)header.Header.DataLength);
        
    }


    private ValueTask SkipPageCountAsync(FileFlags fileFlags)
    {
        return (fileFlags.UnknownPageCount)?ValueTask.CompletedTask:parsingBuffer.AdvanceAsync(4);
    }

    private static bool SegmentIsUnusedInPdf(SegmentHeader readerHeader) => 
        readerHeader.SegmentType is SegmentType.EndOfFile or SegmentType.EndOfPage;

    private bool WantPage(uint headerPage) => headerPage == 0 || headerPage == desiredPage;

    private void WriteSegment(SegmentHeader header, Span<byte> headerSpan, Span<byte> dataSpan)
    {
        var target = header.Page == 0 ? globals : specific;
        if (header.Page is not (0 or 1))
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