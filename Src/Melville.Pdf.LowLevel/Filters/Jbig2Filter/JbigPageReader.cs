using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.FileOrganization;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter;

public abstract class JbigPageReader
{
    private static readonly BinaryBitmap requested = new BinaryBitmap(1, 1);
    protected readonly Dictionary<uint , BinaryBitmap> pages = new();
    protected readonly Dictionary<uint, Segment> storedSegments = new();

    public int TotalPages => pages.Count;
    public void RequestPage(uint page) => pages.Add(page, requested);
    public IBinaryBitmap GetPage(uint page) => pages[page];

    public async ValueTask ProcessFileBitsAsync(PipeReader pipe)
    {
        var segmentHeadReader = await FileHeaderParser.ReadFileHeader(pipe, storedSegments).CA();
        while ((await segmentHeadReader.NextSegmentReader().CA() is { } reader) &&
               reader.Header.SegmentType != SegmentType.EndOfFile)
        {
            if (WantPage(reader.Header.Page))
                ProcessSegment(reader.Header, await reader.ReadFromAsync().CA());
            else
                await reader.SkipOverAsync().CA();
        }
    }

    protected abstract bool WantPage(uint page);

    private void ProcessSegment(SegmentHeader segHead, Segment segment)
    {
        storedSegments[segHead.Number] = segment;
        switch (segment)
        {
            case PageInformationSegment pis:
                HandleSegment(segHead, pis);
                break;
            case RegionSegment rs:
                rs.PlaceIn(pages[segHead.Page]);
                break;
        }
    }

    private void HandleSegment(SegmentHeader segmentHeader, PageInformationSegment pis)
    {
        if (pis.Striping.IsStriped) throw new NotImplementedException("Striped Data");
        pages[segmentHeader.Page] = new BinaryBitmap((int)pis.Height, (int)pis.Width);
    }
}

public class JbigExplicitPageReader : JbigPageReader
{
    protected override bool WantPage(uint page) => IsGlobalSegment(page) || PageWasRequested(page);
    private bool IsGlobalSegment(uint page) => page == 0;
    private bool PageWasRequested(uint page) => pages.ContainsKey(page);
}

public class JbigAllPageReader : JbigPageReader
{
    protected override bool WantPage(uint page) => true;
}