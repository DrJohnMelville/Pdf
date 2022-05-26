using System.Buffers;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class PatternDictionaryParserTest
{
    private static PatternDictionarySegment Parse(byte[] bits)
    {
        var reader = new SequenceReader<byte>(
            new ReadOnlySequence<byte>(
                bits
            )
        );
        return PatternDictionarySegmentParser.Parse(reader);
    }

    [Fact]
    public void ParseExamplePatternDictionary()
    {
        var data =
            "01 04 04 00 00 00 0F 20 D1 84 61 18 45 F2 F9 7C 8F 11 C3 9E 45 F2 F9 7D 42 85 0A AA 84 62 2F EE EC 44 62 22 35 2A 0A 83 B9 DC EE 77 80"
                .BitsFromHex();
        var sut = Parse(data);
        Assert.Equal(16, sut.Patterns.Length);
        Assert.Equal("....\r\n....\r\n....\r\n....", sut.Patterns[0].BitmapString());
        Assert.Equal("....\r\n..B.\r\n....\r\n....", sut.Patterns[1].BitmapString());
        Assert.Equal("....\r\n.BB.\r\n....\r\n....", sut.Patterns[2].BitmapString());
        Assert.Equal("....\r\n.BB.\r\n..B.\r\n....", sut.Patterns[3].BitmapString());
        Assert.Equal("....\r\n.BB.\r\n.BB.\r\n....", sut.Patterns[4].BitmapString());
        Assert.Equal("..B.\r\n.BB.\r\n.BB.\r\n....", sut.Patterns[5].BitmapString());
        Assert.Equal("..B.\r\nBBB.\r\n.BB.\r\n....", sut.Patterns[6].BitmapString());
        Assert.Equal("..B.\r\nBBB.\r\n.BB.\r\n.B..", sut.Patterns[7].BitmapString());
        Assert.Equal("..B.\r\nBBB.\r\n.BBB\r\n.B..", sut.Patterns[8].BitmapString());
        Assert.Equal(".BB.\r\nBBB.\r\nBBBB\r\n.B..", sut.Patterns[9].BitmapString());
        Assert.Equal(".BB.\r\nBBB.\r\nBBBB\r\n.BB.", sut.Patterns[10].BitmapString());
        Assert.Equal(".BB.\r\nBBBB\r\nBBBB\r\n.BB.", sut.Patterns[11].BitmapString());
        Assert.Equal("BBB.\r\nBBBB\r\nBBBB\r\n.BB.", sut.Patterns[12].BitmapString());
        Assert.Equal("BBBB\r\nBBBB\r\nBBBB\r\n.BB.", sut.Patterns[13].BitmapString());
        Assert.Equal("BBBB\r\nBBBB\r\nBBBB\r\n.BBB", sut.Patterns[14].BitmapString());
        Assert.Equal("BBBB\r\nBBBB\r\nBBBB\r\nBBBB", sut.Patterns[15].BitmapString());
        
    }
}