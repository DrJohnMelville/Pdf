
namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class EndOfStripeSegment: Segment 
{
    public uint YCoordinate { get; }

    public EndOfStripeSegment(uint yCoordinate, uint number): base(SegmentType.EndOfStripe, number)
    {
        YCoordinate = yCoordinate;
    }
}