using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.FileOrganization;

namespace Melville.JBig2.Segments;

public class EndOfStripeSegment: Segment 
{
    public uint YCoordinate { get; }

    public EndOfStripeSegment(uint yCoordinate): base(SegmentType.EndOfStripe)
    {
        YCoordinate = yCoordinate;
    }

    public override void HandleSegment(IDictionary<uint, PageBinaryBitmap> pages, uint pageNumber)
    {
        pages[pageNumber].HandleEndOfStripe(YCoordinate);
    }
}