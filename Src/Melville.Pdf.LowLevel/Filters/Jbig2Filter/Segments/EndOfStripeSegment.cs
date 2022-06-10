
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.FileOrganization;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

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