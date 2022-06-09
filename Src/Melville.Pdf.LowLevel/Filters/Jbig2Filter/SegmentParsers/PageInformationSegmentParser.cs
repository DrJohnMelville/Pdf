using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

public static class PageInformationSegmentParser
{
    public static PageInformationSegment Parse(ref SequenceReader<byte> reader)
    {
        var width = reader.ReadBigEndianUint32();
        var height = reader.ReadBigEndianUint32();
        var xResolution = reader.ReadBigEndianUint32();
        var yResolution = reader.ReadBigEndianUint32();
        var flags = new PageInformationFlags(reader.ReadBigEndianUint8());
        var striping = new PageStripingInformation(reader.ReadBigEndianUint16());

        return new (width, height, xResolution, yResolution,flags, striping);
    }
}