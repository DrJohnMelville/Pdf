using System.Buffers;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.Segments;
using Melville.Parsing.SequenceReaders;

namespace Melville.JBig2.SegmentParsers;

public static class EndOfStripeSegmentParser
{
    public static Segment Read(in SegmentHeader header, ref SequenceReader<byte> reader) => 
        new EndOfStripeSegment(reader.ReadBigEndianUint32());
}