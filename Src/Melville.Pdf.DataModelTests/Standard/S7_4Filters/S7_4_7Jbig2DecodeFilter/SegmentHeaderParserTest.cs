using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class SegmentHeaderParserTest
{

    public static object[][] SegmentHeaderExamples() => new object[][]
    {
        new object[]
        {
            new byte[] { 0x00, 0x00, 0x00, 0x20, 0x86, 0x6b, 0x02, 0x1E, 0x05, 0x04, 0x00, 0x00, 0x12, 0x34 },
            new SegmentHeader(32, SegmentType.ImmediateTextRegion, 4, 0x1234)
        },
        new Object[]
        {
            new byte[]
            {
                0x00, 0x00, 0x02, 0x34, 0x40, 0xe0, 0x00, 0x00, 0x09, 0x02, 0xfd, 0x01, 0x00, 0x00,
                0x02, 0x00, 0x1E, 0x00, 0x05, 0x02, 0x00, 0x02, 0x01, 0x02, 0x02, 0x02, 0x03, 0x02,
                0x04, 0x00, 0x00, 0x04, 0x01, 0x12, 0x34, 0x56, 0x78
            },
            new SegmentHeader(564, SegmentType.SymbolDictionary, 1025, 0x12345678)
        }
    };
    
    [Theory]
    [MemberData(nameof(SegmentHeaderExamples))]
    public void ReadSegment(byte[] data, SegmentHeader result)
    {
        var reader = ReaderFromBytes(data);
        Assert.True(SegmentHeaderParser.TryParse(ref reader, out var header));
        RequireEntireBlock(data);
        Assert.Equal(result, header);
    }

    private void RequireEntireBlock(byte[] data)
    {
        for (int i = 0; i < data.Length-1; i++)
        {
            var reader = ReaderFromBytes(data.AsMemory(..i));
            Assert.False(SegmentHeaderParser.TryParse(ref reader, out _));
        }
    }

    private static SequenceReader<byte> ReaderFromBytes(ReadOnlyMemory<byte> data)
    {
        return new SequenceReader<byte>(new ReadOnlySequence<byte>(data));
    }
}