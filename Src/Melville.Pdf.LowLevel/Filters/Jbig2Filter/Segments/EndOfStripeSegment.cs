
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.FileOrganization;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class EndOfStripeSegment: Segment 
{
    public uint YCoordinate { get; }

    public EndOfStripeSegment(uint yCoordinate): base(SegmentType.EndOfStripe)
    {
        YCoordinate = yCoordinate;
    }
}