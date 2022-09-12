using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.SegmentParsers;

namespace Melville.JBig2.Segments;

public class HalftoneSegment : RegionSegment
{
    public HalftoneSegment(SegmentType type, in RegionHeader header, BinaryBitmap bitmap) :
        base(type, in header, bitmap)
    {
    }
}