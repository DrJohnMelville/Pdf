using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.Segments;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.JBig2;

/// <summary>
/// Reads a JBIG page from a stream
/// </summary>
public abstract class JbigPageReader
{
    private static readonly PageBinaryBitmap requested = new(1, 1);
    private protected readonly Dictionary<uint , PageBinaryBitmap> Pages = new();
    private readonly Dictionary<uint, Segment> storedSegments = new();

    /// <summary>
    /// Total number of pages requested by this reader
    /// </summary>
    public int TotalPages => Pages.Count;
    /// <summary>
    /// Request a specific page in the parse operation
    /// </summary>
    /// <param name="page"></param>
    public void RequestPage(uint page) => Pages.Add(page, requested);
    /// <summary>
    /// Get the requested page
    /// </summary>
    /// <param name="page">Zero based page index</param>
    /// <returns>The requested page as a bitmap</returns>
    public IJBigBitmap GetPage(uint page) => Pages[page];

    /// <summary>
    /// Read the requested pages from a stream
    /// </summary>
    /// <param name="stream">The jbig data</param>
    public ValueTask ProcessFileBitsAsync(Stream stream) => ProcessFileBitsAsync((PipeReader)PipeReader.Create(stream)); 
    
    /// <summary>
    /// Read the requested pages from a pipereader
    /// </summary>
    /// <param name="pipe">The source data</param>
    public async ValueTask ProcessFileBitsAsync(PipeReader pipe)
    {
        var segmentHeadReader = await FileHeaderParser.ReadFileHeaderAsync(pipe, storedSegments).CA();
        await ReadSegmentsAsync(segmentHeadReader);
    }

    /// <summary>
    /// Read the s equential segments portion of a JBIG file (which is all that PDF stores
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="pages">The page to read</param>
    public  ValueTask ProcessSequentialSegmentsAsync(Stream stream, uint pages) => 
        ReadSegmentsAsync(new SequentialSegmentHeaderReader(PipeReader.Create(stream), pages, storedSegments));

    private async ValueTask ReadSegmentsAsync(SegmentHeaderReader segmentHeadReader)
    {
        while ((await segmentHeadReader.NextSegmentReaderAsync().CA() is { } reader) &&
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

    /// <summary>
    /// Determine in a page with a given number should be parsed.
    /// </summary>
    /// <param name="page">The page number</param>
    /// <returns>True if the page should be parsed, and false otherwise.</returns>
    protected abstract bool WantPage(uint page);

    private void ProcessSegment(SegmentHeader segHead, Segment segment)
    {
        storedSegments[segHead.Number] = segment;
        segment.HandleSegment(Pages, segHead.Page);
    }
}

/// <summary>
/// A JBigparser than only parses the requested pages
/// </summary>
public class JbigExplicitPageReader : JbigPageReader
{
    /// <inheritdoc />
    protected override bool WantPage(uint page) => IsGlobalSegment(page) || PageWasRequested(page);
    private bool IsGlobalSegment(uint page) => page == 0;
    private bool PageWasRequested(uint page) => Pages.ContainsKey(page);
}

/// <summary>
/// A JBigapage reader that reads every page
/// </summary>
public class JbigAllPageReader : JbigPageReader
{
    /// <inheritdoc />
    protected override bool WantPage(uint page) => true;
}