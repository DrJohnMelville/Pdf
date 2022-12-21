using System.Collections.Generic;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.SegmentParsers;

namespace Melville.JBig2.Segments;

internal class RegionSegment: Segment
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

    public override void HandleSegment(IDictionary<uint, PageBinaryBitmap> pages, uint pageNumber) => 
        PlaceIn(pages[pageNumber]);

    public void PlaceIn(BinaryBitmap target) => 
        target.PasteBitsFrom((int)Y, (int)X, Bitmap, CombinationOperator);
}