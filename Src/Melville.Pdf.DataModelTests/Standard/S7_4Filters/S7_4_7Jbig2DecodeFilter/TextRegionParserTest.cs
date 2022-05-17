
using System;
using System.Buffers;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class BitAssignmentTest
{
    public static object[][] BitAssignmentPatterns() =>
        new[]
        {
            new object[]{new int[]{1,2,3,0,3}, new []{0,2,6,0,7}}, // b1
            new object[]{new int[]{1,2,3,4,5,6,6}, new []{0,0b10, 0b110,0b1110, 0b11110, 0b111110, 0b111111}}, // b2
            new object[]{new int[]{8,1,2,3,4,5,8,7,6}, new []{0b11111110, 0, 0b10,0b110, 0b1110, 0b11110, 0xFF, 0b1111110, 0b111110}}, // b3
            new object[]{new int[]{1,2,3,4,5,5}, new []{0,2,6, 14, 30, 31}}, // b4
            new object[]{new int[]{7,1,2,3,4,5,7,6}, new []{126, 0, 2, 6, 14, 30, 127, 62}}, // b5
            new object[]{new int[]{5, 4, 4, 4, 5, 5, 4, 2,3,3,4,4,6,6},
                new []{0b11100, 0b1000, 0b1001, 0b1010,0b11101, 0b11110, 0b1011, 0, 2, 3, 0b1100, 0b1101, 0b111110, 0b111111}}, // b6
            new object[]{new int[]{4,3,4,5,5,4,4,5,5,4,3,3,3,5,5},
                new []{0b1000, 0, 0b1001, 0b11010, 0b11011, 0b1010, 0b1011, 0b11100, 0b11101, 0b1100, 0b001, 0b010, 0b011, 0b11110, 0b11111}}, // b7
            new object[]{new int[]{8,9,8,9,7,4,2,5,6, 3,6,4,4,5,5,6,7,6,9,9,2},
                new []{0b11111100, 0b111111100, 0b11111101, 0b111111101, 0b1111100, 0b1010,0,0b11010, 0b111010, 0b100, 
                    0b111011, 0b1011, 0b1100, 0b11011, 0b11100, 0b111100, 0b1111101, 0b111101, 0b111111110, 0b111111111,0b01}}, // b8
            new object[]{new int[]{8,9,8,9,7,4,3,3,5,6,3,6,4,4,5,5,6,7,6,9,9,2},
                new []
                { 0b11111100, 0b111111100, 0b11111101, 0b111111101, 0b1111100, 0b1010, 0b010, 0b011, 0b11010,
                    0b111010, 0b100, 0b111011, 0b1011, 0b1100, 0b11011, 0b11100, 0b111100, 0b1111101, 0b111101,
                    0b111111110, 0b111111111, 0b00 }}, // b9
            new object[]{new int[]{8,9,8,9,7,4,3,3,5,6,3,6,4,4,5,5,6,7,6,9,9,2},
                new []
                { 0b11111100, 0b111111100, 0b11111101, 0b111111101, 0b1111100, 0b1010, 0b010, 0b011, 0b11010,
                    0b111010, 0b100, 0b111011, 0b1011, 0b1100, 0b11011, 0b11100, 0b111100, 0b1111101, 0b111101,
                    0b111111110, 0b111111111, 0b00 }}, // 10
        };

    [Theory]
    [MemberData(nameof(BitAssignmentPatterns))]
    public void AssignBitLengths(int[] lengths, int[] result)
    {
        Assert.Equal(lengths.Length, result.Length);
        
        var codes = new int[result.Length];
        BitAssignment.AssignPrefixes(lengths, codes);
        for (int i = 0; i < lengths.Length; i++)
        {
            Assert.Equal(result[i], codes[i]);
            
        }
        Assert.Equal(result, codes);
        
    }
}
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