using System.Collections.Generic;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.FileOrganization;

namespace Melville.JBig2.Segments;

internal class Segment
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