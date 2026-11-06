using System.Buffers;
using Melville.JBig2.Segments;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.JBig2.SegmentParsers;

internal static class PageInformationSegmentParser
{
    public static PageInformationSegment Parse(ref SpanParsingBuffer reader)
    {
        var width = reader.GetUInt32BigEndian();
        var height = reader.GetUInt32BigEndian();
        var xResolution = reader.GetUInt32BigEndian();
        var yResolution = reader.GetUInt32BigEndian();
        var flags = new PageInformationFlags(reader.GetUInt8());
        var striping = new PageStripingInformation(reader.GetUInt16BigEndian());

        return new (width, height, xResolution, yResolution,flags, striping);
    }
}