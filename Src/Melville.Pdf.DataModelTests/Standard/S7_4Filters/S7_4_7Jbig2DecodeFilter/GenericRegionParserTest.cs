using System;
using System.Buffers;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.GenericRegionParsers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class GenericRegionParserTest
{
    private static GenericRegionSegment Parse(byte[] bits)
    {
        var reader = new SequenceReader<byte>(
            new ReadOnlySequence<byte>(
                bits
            )
        );
        return GenericRegionSegmentParser.Parse(reader, Array.Empty<Segment>());
    }

    [Theory]
    [InlineData("00000036 0000002C 00000004 0000000B 00 " + // region header
                "01 26 A0 71 CE A7 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF F8 F0")]
    [InlineData("00000036 0000002C 00000004 0000000B 00 " + // region headder
                "08 03 FF FD FF 02 FE FE FE 04 EE ED 87 FB CB 2B FF AC")]
    public void ParseGenericSegment(string data)
    {
        var sut = Parse(data.BitsFromHex());
        Assert.Equal(54, sut.Bitmap.Width);
        Assert.Equal(44, sut.Bitmap.Height);
        Assert.Equal(4u, sut.X);
        Assert.Equal(11u, sut.Y);
        Assert.Equal(CombinationOperator.Or, sut.CombinationOperator);
        Assert.Equal(@"
BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB
BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BB..................................................BB
BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB
BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB", "\r\n"+sut.Bitmap.BitmapString());

    }
}