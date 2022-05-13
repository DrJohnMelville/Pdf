using System.Buffers;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

public static class TextRegionSegmentParser
{
    public static TextRegionSegment Parse(ref SequenceReader<byte> reader, uint segmentNumber)
    {
        var regionHead = RegionHeaderParser.Parse(ref reader);
        return new TextRegionSegment(SegmentType.IntermediateTextRegion, segmentNumber,
            new BinaryBitmap((int)regionHead.Height, (int)regionHead.Width));
    }
}

