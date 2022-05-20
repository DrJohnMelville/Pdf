using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

public static class EndOfStripeSegmentParser
{
    public static Segment Read(in SegmentHeader header, ref SequenceReader<byte> reader) => 
        new EndOfStripeSegment(reader.ReadBigEndianUint32());
}