using System.Buffers;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.Segments;
using Melville.Parsing.SequenceReaders;

namespace Melville.JBig2.SegmentParsers;

internal readonly record struct RegionHeader(
    uint Width, uint Height, uint X, uint Y, CombinationOperator CombinationOperator)
{
    public BinaryBitmap CreateTargetBitmap() => new((int)Height, (int)Width);
}

internal static class RegionHeaderParser
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