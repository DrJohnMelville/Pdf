using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.FileOrganization;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class RegionSegment: Segment
{
    public uint X { get; }
    public uint Y { get; }
    public CombinationOperator CombinationOperator { get; }
    public BinaryBitmap Bitmap { get; }

    protected RegionSegment(SegmentType type, in RegionHeader header, BinaryBitmap bitmap) : base(type)
    {
        Bitmap = bitmap;
        X = header.X;
        Y = header.Y;
        CombinationOperator = header.CombinationOperator;
    }

    public void PlaceIn(BinaryBitmap target) => target.PasteBitsFrom((int)Y, (int)X, Bitmap, CombinationOperator);
}