
using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

public record struct RegionHeader(
    uint Width, uint Height, uint X, uint Y, CombinationOperator CombinationOperator);

public static class RegionHeaderParser
{
    public static RegionHeader Parse(ref SequenceReader<byte> reader)
    {
        var width = reader.ReadBigEndianUint32();
        var height = reader.ReadBigEndianUint32();
        var x = reader.ReadBigEndianUint32();
        var y = reader.ReadBigEndianUint32();
        var combinationOperator = (CombinationOperator)reader.ReadBigEndianUint8();
        return new RegionHeader(width, height, x, y, combinationOperator);
    }
}