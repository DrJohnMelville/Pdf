using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.FileOrganization;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class GenericRefinementRegionSegment: RegionSegment
{
    public GenericRefinementRegionSegment(SegmentType type, in RegionHeader header, BinaryBitmap bitmap) :
        base(type, in header, bitmap)
    {
    }
}