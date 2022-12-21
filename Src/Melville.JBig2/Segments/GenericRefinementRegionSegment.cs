using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.SegmentParsers;

namespace Melville.JBig2.Segments;

internal class GenericRefinementRegionSegment: RegionSegment
{
    public GenericRefinementRegionSegment(SegmentType type, in RegionHeader header, BinaryBitmap bitmap) :
        base(type, in header, bitmap)
    {
    }
}