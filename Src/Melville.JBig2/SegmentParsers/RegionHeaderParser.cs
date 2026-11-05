using System;
using System.Buffers;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.Segments;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.JBig2.SegmentParsers;

internal readonly record struct RegionHeader(
    uint Width, uint Height, uint X, uint Y, CombinationOperator CombinationOperator)
{
    public BinaryBitmap CreateTargetBitmap() => new((int)Height, (int)Width);
}

internal static class RegionHeaderParser
{
#warning remove this
    [Obsolete]
    public static RegionHeader Parse(ref SequenceReader<byte> reader)
    {
        var width = reader.GetUInt32BigEndian();
        var height = reader.GetUInt32BigEndian();
        var x = reader.GetUInt32BigEndian();
        var y = reader.GetUInt32BigEndian();
        var combinationOperator = (CombinationOperator)reader.GetUInt8BigEndian();
        return new RegionHeader(width, height, x, y, combinationOperator);
    }
    public static RegionHeader Parse(ref SpanParsingBuffer reader)
    {
        var width = reader.GetUInt32BigEndian();
        var height = reader.GetUInt32BigEndian();
        var x = reader.GetUInt32BigEndian();
        var y = reader.GetUInt32BigEndian();
        var combinationOperator = (CombinationOperator)reader.GetUInt8();
        return new RegionHeader(width, height, x, y, combinationOperator);
    }
}