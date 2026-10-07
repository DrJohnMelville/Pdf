using System.Buffers;
using Melville.JBig2.Segments;
using Melville.Parsing.SequenceReaders;

namespace Melville.JBig2.SegmentParsers;

internal static class PageInformationSegmentParser
{
    public static PageInformationSegment Parse(ref SequenceReader<byte> reader)
    {
        var width = reader.GetUInt32BigEndian();
        var height = reader.GetUInt32BigEndian();
        var xResolution = reader.GetUInt32BigEndian();
        var yResolution = reader.GetUInt32BigEndian();
        var flags = new PageInformationFlags(reader.GetUInt8BigEndian());
        var striping = new PageStripingInformation(reader.GetUInt16BigEndian());

        return new (width, height, xResolution, yResolution,flags, striping);
    }
}