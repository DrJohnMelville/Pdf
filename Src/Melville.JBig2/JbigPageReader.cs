using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.Segments;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.JBig2;

public abstract class JbigPageReader
{
    private static readonly PageBinaryBitmap requested = new(1, 1);
    protected readonly Dictionary<uint , PageBinaryBitmap> Pages = new();
    private readonly Dictionary<uint, Segment> storedSegments = new();

    public int TotalPages => Pages.Count;
    public void RequestPage(uint page) => Pages.Add(page, requested);
    public BinaryBitmap GetPage(uint page) => Pages[page];

    public ValueTask ProcessFileBitsAsync(Stream stream) => ProcessFileBitsAsync((PipeReader)PipeReader.Create(stream)); 
    public async ValueTask ProcessFileBitsAsync(PipeReader pipe)
    {
        var segmentHeadReader = await FileHeaderParser.ReadFileHeader(pipe, storedSegments).CA();
        await ReadSegments(segmentHeadReader);
    }

    public  ValueTask ProcessSequentialSegments(Stream stream, uint pages) => 
        ReadSegments(new SequentialSegmentHeaderReader(PipeReader.Create(stream), pages, storedSegments));

    private async ValueTask ReadSegments(SegmentHeaderReader segmentHeadReader)
    {
        while ((await segmentHeadReader.NextSegmentReader().CA() is { } reader) &&
               reader.Header.SegmentType != SegmentType.EndOfFile)
        {
            if (WantPage(reader.Header.Page))
                ProcessSegment(reader.Header, await reader.ReadFromAsync().CA());
            else
                await reader.SkipOverAsync().CA();
        }

        FinalizePages();
    }

    private void FinalizePages()
    {
        foreach (var page in Pages.Values)
        {
            page.DoneReading();
        }
    }

    protected abstract bool WantPage(uint page);

    private void ProcessSegment(SegmentHeader segHead, Segment segment)
    {
        storedSegments[segHead.Number] = segment;
        segment.HandleSegment(Pages, segHead.Page);
    }
}

public class JbigExplicitPageReader : JbigPageReader
{
    protected override bool WantPage(uint page) => IsGlobalSegment(page) || PageWasRequested(page);
    private bool IsGlobalSegment(uint page) => page == 0;
    private bool PageWasRequested(uint page) => Pages.ContainsKey(page);
}

public class JbigAllPageReader : JbigPageReader
{
    protected override bool WantPage(uint page) => true;
}