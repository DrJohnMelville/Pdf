using System.Buffers;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class StandardHuffmanTest
{
    [Theory]
    [InlineData(HuffmanTableSelection.B1, "0 0001 0 1111", 1,15)]
    [InlineData(HuffmanTableSelection.B1, "10 00000001 10 11111111", 17,271)]
    [InlineData(HuffmanTableSelection.B1, "110 00000000 00000001 110 11111111 11111111", 273,65807)]
    [InlineData(HuffmanTableSelection.B1, 
        "111 00000000 00000000 00000000 00000001 111 00000000 00000000 00000000 00000011 ", 65809,65811)]
    
    [InlineData(HuffmanTableSelection.B2, "00", 0,0 )]
    [InlineData(HuffmanTableSelection.B2, "1010", 1,1 )]
    [InlineData(HuffmanTableSelection.B2, "11010", 2,1 )]
    [InlineData(HuffmanTableSelection.B2, "1110 001 1110 111", 4,10 )]
    [InlineData(HuffmanTableSelection.B2, "11110 000001 11110 111111", 12,74 )]
    [InlineData(HuffmanTableSelection.B2, "111110 0000000000000000 0000000000000001 0", 76,0 )]
    
    [InlineData(HuffmanTableSelection.B3, "0 10", 0, 1 )]
    [InlineData(HuffmanTableSelection.B3, "110 1110 000", 2, 3 )]
    [InlineData(HuffmanTableSelection.B3, "1110 001 1110 111", 4, 10 )]
    [InlineData(HuffmanTableSelection.B3, "11110 000001 11110 111111", 12, 74 )]
    [InlineData(HuffmanTableSelection.B3, "111110 111110", int.MaxValue, int.MaxValue )]
    [InlineData(HuffmanTableSelection.B3, 
        "1111110 00000000000000000000000000000001 1111110 00000000000000000000000000000011", 76, 78 )]
    [InlineData(HuffmanTableSelection.B3, "11111110 00000001 11111110 11111111", -255, -1 )]
    [InlineData(HuffmanTableSelection.B3, 
        "11111111 00000000000000000000000000000001 11111111 00000000000000000000000000000011", -258, -260 )]
    
    [InlineData(HuffmanTableSelection.B4, "010", 1, 2)]
    [InlineData(HuffmanTableSelection.B4, "1100", 3, 1)]
    [InlineData(HuffmanTableSelection.B4, "1110 001 1110 111", 5, 11)]
    [InlineData(HuffmanTableSelection.B4, "11110 000001 11110 111111", 13, 75)]
    [InlineData(HuffmanTableSelection.B4, 
        "11111 00000000000000000000000000000001 11111 00000000000000000000000000000011", 77, 79)]
    
    [InlineData(HuffmanTableSelection.B5, "0 10", 1, 2)]
    [InlineData(HuffmanTableSelection.B5, "110 0", 3, 1)]
    [InlineData(HuffmanTableSelection.B5, "1110 001 1110 111", 5, 11)]
    [InlineData(HuffmanTableSelection.B5, "11110 000001 11110 111111", 13, 75)]
    [InlineData(HuffmanTableSelection.B5, 
        "111110 00000000000000000000000000000001 111110 00000000000000000000000000000011", 77, 79)]
    [InlineData(HuffmanTableSelection.B5, "1111110 00000001 1111110 11111111", -254, 0)]
    [InlineData(HuffmanTableSelection.B5, 
        "1111111 00000000000000000000000000000001 1111111 00000000000000000000000000000011", -257, -259)]
    
    [InlineData(HuffmanTableSelection.B6, "00 0000001 00 1111111", 1, 127)]
    [InlineData(HuffmanTableSelection.B6, "010 0000001 010 1111111", 129, 255)]
    [InlineData(HuffmanTableSelection.B6, "011 00000001 011 11111111", 257, 511)]
    [InlineData(HuffmanTableSelection.B6, "1000 000000001 1000 111111111", -1023, -513)]
    [InlineData(HuffmanTableSelection.B6, "1001 00000001 1001 11111111", -511, -257)]
    [InlineData(HuffmanTableSelection.B6, "1010 0000001 1010 1111111", -255, -129)]
    [InlineData(HuffmanTableSelection.B6, "1100 000000001 1100 111111111", 513, 1023)]
    [InlineData(HuffmanTableSelection.B6, "1101 0000000001 1101 1111111111", 1025, 2047)]
    [InlineData(HuffmanTableSelection.B6, "11101 000001 11101 111111", -127, -65)]
    [InlineData(HuffmanTableSelection.B6, "11110 00001 11110 11111", -63, -33)]
    [InlineData(HuffmanTableSelection.B6, "11100 0000000001 11100 1111111111", -2047, -1025)]
    [InlineData(HuffmanTableSelection.B6, "111110 00000000000000000000000000000001 111110 00000000000000000000000000000011", -2050, -2052)]
    [InlineData(HuffmanTableSelection.B6, "111111 00000000000000000000000000000001 111111 00000000000000000000000000000011", 2049, 2051)]
   
    [InlineData(HuffmanTableSelection.B7, "000 00000001 000 11111111", -511, -257)]
    [InlineData(HuffmanTableSelection.B7, "001 00000001 001 11111111", 257, 511)]
    [InlineData(HuffmanTableSelection.B7, "011 0000000001 011 1111111111", 1025, 2047)]
    [InlineData(HuffmanTableSelection.B7, "1000 000000001 1000 111111111", -1023, -513)]
    [InlineData(HuffmanTableSelection.B7, "1001 0000001 1001 1111111", -255, -129)]
    [InlineData(HuffmanTableSelection.B7, "1010 00001 1010 11111", -31, -1)]
    [InlineData(HuffmanTableSelection.B7, "1011 00001 1011 11111", 1, 31)]
    [InlineData(HuffmanTableSelection.B7, "1100 0000001 1100 1111111", 129, 255)]
    [InlineData(HuffmanTableSelection.B7, "11010 000001 11010 111111", -127, -65)]
    [InlineData(HuffmanTableSelection.B7, "11011 00001 11011 11111", -63, -33)]
    [InlineData(HuffmanTableSelection.B7, "11110 00000000000000000000000000000001 11110 00000000000000000000000000000011", -1026, -1028)]
    [InlineData(HuffmanTableSelection.B7, "11111 00000000000000000000000000000001 11111 00000000000000000000000000000011", 2049, 2051)]
    
    [InlineData(HuffmanTableSelection.B8, "00 0 01", 0, int.MaxValue)]
    [InlineData(HuffmanTableSelection.B8, "10101010", -1, -1)]
    [InlineData(HuffmanTableSelection.B8, "1011 0001 1011 1111", 23, 37)]
    [InlineData(HuffmanTableSelection.B8, "1100 00001 1100 11111", 39, 69)]
    [InlineData(HuffmanTableSelection.B8, "11010 11010", 2,2)]
    [InlineData(HuffmanTableSelection.B8, "11011 000001 11011 111111", 71, 133)]
    [InlineData(HuffmanTableSelection.B8, "11100 0000001 11100 1111111", 135,261)]
    [InlineData(HuffmanTableSelection.B8, "111100 0000001 111100 1111111", 263, 389)]
    [InlineData(HuffmanTableSelection.B8, "111101 0000000001 111101 1111111111", 647, 1669)]
    [InlineData(HuffmanTableSelection.B8, "1111100 1111100", -2, -2)]
    [InlineData(HuffmanTableSelection.B8, "11111100 001 11111100 111", -14, -8)]
    [InlineData(HuffmanTableSelection.B8, "11111101 0 11111101 1", -5, -4)]
    [InlineData(HuffmanTableSelection.B8, "111111101 111111101", -3, -3)]
    [InlineData(HuffmanTableSelection.B8, "111111110 00000000000000000000000000000001 111111110 00000000000000000000000000000011", -17, -19)]
    [InlineData(HuffmanTableSelection.B8, "111111111 00000000000000000000000000000001 111111111 00000000000000000000000000000011", 1671, 1673)]
    
    [InlineData(HuffmanTableSelection.B9, "0000", int.MaxValue, int.MaxValue)]
    [InlineData(HuffmanTableSelection.B9, "010 0 010 1", -1, 0)]
    [InlineData(HuffmanTableSelection.B9, "011 0 011 1", 1, 2)]
    [InlineData(HuffmanTableSelection.B9, "100 00001 100 11111", 8, 38)]
    [InlineData(HuffmanTableSelection.B9, "1010 0 1010 1", -3, -2)]
    [InlineData(HuffmanTableSelection.B9, "1011 00001 1011 11111", 44, 74)]
    [InlineData(HuffmanTableSelection.B9, "1100 000001 1100 111111", 76, 138)]
    [InlineData(HuffmanTableSelection.B9, "11010 0 11010 1", 3, 4)]
    [InlineData(HuffmanTableSelection.B9, "11011 0000001 11011 1111111", 140, 266)]
    [InlineData(HuffmanTableSelection.B9, "11100 00000001 11100 11111111", 268, 522)]
    [InlineData(HuffmanTableSelection.B9, "111011 01 111011 11", 40, 42)]
    [InlineData(HuffmanTableSelection.B9, "111100 00000001 111100 11111111", 524,778)]
    [InlineData(HuffmanTableSelection.B9, "111101 00000000001 111101 11111111111", 1292, 3338)]
    [InlineData(HuffmanTableSelection.B9, "1111100 0 1111100 1", -5, -4)]
    [InlineData(HuffmanTableSelection.B9, "1111101 000000001 1111101 111111111", 780, 1290)]
    [InlineData(HuffmanTableSelection.B9, "11111100 0001 11111100 1111", -30, -16)]
    [InlineData(HuffmanTableSelection.B9, "11111101 01 11111101 11", -10, -8)]
    [InlineData(HuffmanTableSelection.B9, "111111101 0 111111101 1", -7, -6)]
    [InlineData(HuffmanTableSelection.B9, "111111110 00000000000000000000000000000001 111111110 00000000000000000000000000000011", -33, -35)]
    [InlineData(HuffmanTableSelection.B9, "111111111 00000000000000000000000000000001 111111111 00000000000000000000000000000011", 3340, 3342)]
    public void ReadStandardHuffman(HuffmanTableSelection tableSelector, string data, int firstNum, int secondNum)
    {
        var table = StandardHuffmanTables.FromSelector(tableSelector);
        var source = new BitSource(new SequenceReader<byte>(new ReadOnlySequence<byte>(data.BitsFromBinary())));
        Assert.Equal(firstNum, table.GetInteger(ref source));
        Assert.Equal(secondNum, table.GetInteger(ref source));
    }
}