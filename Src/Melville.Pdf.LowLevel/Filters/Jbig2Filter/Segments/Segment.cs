using System.Collections.Generic;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.FileOrganization;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class Segment
{
    public static readonly Segment EndOfPage = new(SegmentType.EndOfPage);
    public static readonly Segment EndOfFile = new(SegmentType.EndOfFile);
    public SegmentType Type { get; }

    protected Segment(SegmentType type)
    {
        Type = type;
    }

    public virtual void HandleSegment(IDictionary<uint, PageBinaryBitmap> pages, uint pageNumber)
    {
    }
}