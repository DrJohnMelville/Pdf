using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.SegmentParsers;

namespace Melville.JBig2.Segments;

internal class GenericRegionSegment: RegionSegment{
    public GenericRegionSegment(SegmentType type, in RegionHeader header, BinaryBitmap bitmap) : base(type, in header, bitmap)
    {
    }
}