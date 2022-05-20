
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class RegionSegment: Segment
{
    public BinaryBitmap Bitmap { get; }

    protected RegionSegment(SegmentType type, BinaryBitmap bitmap) : base(type)
    {
        Bitmap = bitmap;
    }
}