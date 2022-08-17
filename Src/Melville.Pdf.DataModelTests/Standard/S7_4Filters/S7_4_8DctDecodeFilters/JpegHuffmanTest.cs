using System;
using System.Buffers;
using System.Runtime.Intrinsics.X86;
using Melville.Pdf.LowLevel.Filters.Jpeg;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_8DctDecodeFilters;

public class JpegHuffmanTest
{
    [Theory]
    [InlineData("00", 65)]
    [InlineData("01", 66)]
    [InlineData("100", 67)]
    [InlineData("1010", 68)]
    [InlineData("1011", 69)]
    [InlineData("11000", 70)]
    [InlineData("11001", 71)]
    [InlineData("11010", 72)]
    [InlineData("110110", 73)]
    [InlineData("110111", 74)]
    [InlineData("111000", 75)]
    [InlineData("111001", 76)]
    [InlineData("111010", 77)]
    [InlineData("111011", 78)]
    [InlineData("111100", 79)]
    [InlineData("111101", 80)]
    [InlineData("111110", 81)]
    public void DecodeHuffman1(string input, byte result)
    {
        var source = new byte[]
        {
            0, 2, 1, 2, 3, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81
        };
        var huffmanTable = ReadTable(source);
        
        CheckCode(input, result, huffmanTable);
    }

    [Theory]
    [InlineData("00", 65)]
    [InlineData("010", 66)]
    [InlineData("011", 67)]
    [InlineData("100", 68)]
    [InlineData("101", 69)]
    [InlineData("110", 70)]
    [InlineData("1110", 71)]
    [InlineData("11110", 72)]
    [InlineData("111110", 73)]
    [InlineData("1111110", 74)]
    [InlineData("11111110", 75)]
    [InlineData("111111110", 76)]
    [InlineData("111111111", 77)]
    public void DecodeHuffman12(string input, byte result)
    {
        var source = new byte[]
        {
            0, 1, 5, 1, 1, 1, 1, 1, 2, 0, 0, 0, 0, 0, 0, 0,
            65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81
        };
        var huffmanTable = ReadTable(source);
        
        CheckCode(input, result, huffmanTable);
    }

    private static void CheckCode(string input, byte result, HuffmanTable huffmanTable)
    {
        var inputSeq = new SequenceReader<byte>(new ReadOnlySequence<byte>(input.BitsFromBinary()));
        var bitReader = new BitReader();
        Assert.Equal(result, huffmanTable.Read(ref inputSeq, bitReader));
    }

    private static HuffmanTable ReadTable(byte[] source)
    {
        var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(source));
        var huffmanTable = JpegStreamFactory.HuffmanTableParser.Instance.ParseTable(ref reader);
        return huffmanTable;
    }
}