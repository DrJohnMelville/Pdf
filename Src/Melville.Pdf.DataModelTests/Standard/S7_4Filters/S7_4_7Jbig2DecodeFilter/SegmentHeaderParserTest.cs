using System;
using System.Buffers;
using Melville.JBig2.FileOrganization;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class SegmentHeaderParserTest
{

    public static object[][] SegmentHeaderExamples() => new object[][]
    {
        new object[]
        {
            "00000020 86 6B 02 1E 05 04 00001234".BitsFromHex(),
            new SegmentHeader(32, SegmentType.ImmediateTextRegion, 4, 0x1234, new uint[]{2,30,5})
        },
        new Object[]
        {
            ("00000234" +// id number
            "40" + // flags
            "e0 00 00 09" + // 9 referred segments in long format
            "02 fd" + // retain bits
            "0100 0002 001E 0005 0200 0201 0202 0203 0204" + // referred segments
            "00 00 04 01" + // page number
            "12345678") // segment length
                .BitsFromHex(),
            new SegmentHeader(564, SegmentType.SymbolDictionary, 1025, 0x12345678, 
                new uint[]{256,2,0x1e, 5, 0x200, 0x201, 0x202, 0x203, 0x204})
        },
        
        new Object[]
        {
            @"0F00023440e000000902fd00000100000000020000001E0000000500000200000002010000020200000203000002040000040112345678".BitsFromHex(),
            new SegmentHeader(0x0f000234, SegmentType.SymbolDictionary, 1025, 0x12345678, 
                new uint[]{256,2,0x1e, 5, 0x200, 0x201, 0x202, 0x203, 0x204})
        }
    };
    
    [Theory]
    [MemberData(nameof(SegmentHeaderExamples))]
    public void ReadSegment(byte[] data, SegmentHeader result)
    {
        var reader = ReaderFromBytes(data);
        Assert.True(SegmentHeaderParser.TryParse(ref reader, out var header));
        RequireEntireBlock(data);
        Assert.Equal(result.Number, header.Number);
        Assert.Equal(result.SegmentType, header.SegmentType);
        Assert.Equal(result.ReferencedSegmentNumbers, header.ReferencedSegmentNumbers);
        Assert.Equal(result.Page, header.Page);
        Assert.Equal(result.DataLength, header.DataLength);
        
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