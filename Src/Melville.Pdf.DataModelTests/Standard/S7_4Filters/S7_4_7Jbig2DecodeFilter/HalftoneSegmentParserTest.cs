using System;
using System.Buffers;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.HalftoneRegionParsers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class HalftoneSegmentParserTest
{
    private static HalftoneSegment Parse(byte[] bits)
    {
        var reader = new SequenceReader<byte>(
            new ReadOnlySequence<byte>(
                bits
            )
        );
        ReadOnlySpan<Segment> segs = new Segment[] { PatternDictionaryParserTest.SampleSegment() };
        return HalftoneSegmentParser.Parse(reader, segs);
    }

    [Fact]
    public void ParseExampleHalftoneRegion()
    {
        var data = @"00000020 00000024 00000010 0000000F 00 
                   01 00000008 00000009 00000000 00000000 0400 0000 AA AA AA AA 80 08 00 80 36 D5
                   55 6B 5A D4 00 40 04 2E E9 52 D2 D2 D2 8A A5 4A
                   00 20 02 23 E0 95 24 B4 92 8A 4A 92 54 92 D2 4A
                   29 2A 49 40 04 00 40".BitsFromHex();
        var sut = Parse(data);
        Assert.Equal(32, sut.Bitmap.Width);
        Assert.Equal(36, sut.Bitmap.Height);
        Assert.Equal(16u, sut.X);
        Assert.Equal(15u, sut.Y);
        Assert.Equal(CombinationOperator.Or, sut.CombinationOperator);
        Assert.Equal(@"
......................B...B...B.
......B..BB..BB..BB..BB.BBB.BBB.
..............B..BB..BB..BB..BB.
.............................B..
..................B...B...B...B.
..B..BB..BB..BB..BB.BBB.BBB.BBB.
..........B..BB..BB..BB..BB..BBB
.........................B...B..
..............B...B...B...B..BB.
.BB..BB..BB..BB.BBB.BBB.BBB.BBB.
......B..BB..BB..BB..BB..BBBBBBB
.....................B...B...B..
..........B...B...B...B..BB..BB.
.BB..BB..BB.BBB.BBB.BBB.BBB.BBB.
..B..BB..BB..BB..BB..BBBBBBBBBBB
.................B...B...B...BB.
......B...B...B...B..BB..BB..BB.
.BB..BB.BBB.BBB.BBB.BBB.BBB.BBBB
.BB..BB..BB..BB..BBBBBBBBBBBBBBB
.............B...B...B...BB..BB.
..B...B...B...B..BB..BB..BB.BBB.
.BB.BBB.BBB.BBB.BBB.BBB.BBBBBBBB
.BB..BB..BB..BBBBBBBBBBBBBBBBBBB
.........B...B...B...BB..BB..BB.
..B...B...B..BB..BB..BB.BBB.BBBB
BBB.BBB.BBB.BBB.BBB.BBBBBBBBBBBB
.BB..BB..BBBBBBBBBBBBBBBBBBBBBBB
.....B...B...B...BB..BB..BB..BB.
..B...B..BB..BB..BB.BBB.BBBBBBBB
BBB.BBB.BBB.BBB.BBBBBBBBBBBBBBBB
.BB..BBBBBBBBBBBBBBBBBBBBBBBBBBB
.B...B...B...BB..BB..BB..BB..BBB
..B..BB..BB..BB.BBB.BBBBBBBBBBBB
BBB.BBB.BBB.BBBBBBBBBBBBBBBBBBBB
.BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB
.B...B...BB..BB..BB..BB..BBBBBBB", "\r\n"+sut.Bitmap.BitmapString());
        
    }
}