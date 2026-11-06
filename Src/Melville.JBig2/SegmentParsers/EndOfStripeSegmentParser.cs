using System.Buffers;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.Segments;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.JBig2.SegmentParsers;

internal static class EndOfStripeSegmentParser
{
    public static Segment Read(scoped in SegmentHeader header, scoped ref SpanParsingBuffer reader) => 
        new EndOfStripeSegment(reader.GetUInt32BigEndian());
}