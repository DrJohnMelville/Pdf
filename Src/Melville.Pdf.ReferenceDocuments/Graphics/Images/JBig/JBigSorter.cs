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
        if (!fileFlags.SequentialFileOrganization)
            throw new NotImplementedException("read only organization");
        while(ParseSequential()); // empty loop is intentional
    }

    private bool ParseSequential()
    {
        var sr = new SequenceReader<byte>(new ReadOnlySequence<byte>(sourceBuffer.AsMemory(pos)));
        var sp = sr.Position;
        if (!SegmentHeaderParser.TryParse(ref sr, out var header) || header.SegmentType == SegmentType.EndOfFile)
            return false;
        var length = sr.Position.GetInteger() - sp.GetInteger();
        var headerSpan = sourceBuffer.AsSpan(pos, length);
        pos += length;
        var dataSpan = sourceBuffer.AsSpan(pos, (int)header.DataLength);
        pos += dataSpan.Length;
        if (WantPage(header.Page) && !SegmentIsUnusedInPdf(header))
        {
            WriteSegment(header, headerSpan, dataSpan);
        }
        return true;
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