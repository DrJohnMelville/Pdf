
using System.Buffers;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class TextRegionParserTest
{
    private static TextRegionSegment Parse(byte[] bits)
    {
        var reader = new SequenceReader<byte>(
            new ReadOnlySequence<byte>(
                bits
            )
        );
        return TextRegionSegmentParser.Parse(ref reader, 210);
    }
    [Fact]
    public void ParseTextSegment()
    {
        var data = "00000025 00000008 00000004 00000001 00 0C09 0010 00000005 01100000000000000000000000000000000C" +
                   "4007087041D0";
        var sut = Parse(data.BitsFromHex());
        Assert.Equal(0x25, sut.Bitmap.Width);
        Assert.Equal(8, sut.Bitmap.Height);
        
    }

    [Fact]
    public void ParseRegionHeader()
    {
        var data = "00000025 00000008 00000004 00000001 01";
        var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(data.BitsFromHex()));
        var region = RegionHeaderParser.Parse(ref reader);
        Assert.Equal(0x25u, region.Width);
        Assert.Equal(0x08u, region.Height);
        Assert.Equal(0x04u, region.X);
        Assert.Equal(0x01u, region.Y);
        Assert.Equal(CombinationOperator.And, region.CombinationOperator);
        
    }
}