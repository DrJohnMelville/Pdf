using System;
using System.Runtime.InteropServices;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class BitCopierTest {

    [Theory]
    [InlineData(0, 0b0000_0000, 0b1111_1111)]
    [InlineData(1, 0b1000_0000, 0b0111_1111)]
    [InlineData(2, 0b1100_0000, 0b0011_1111)]
    [InlineData(3, 0b1110_0000, 0b0001_1111)]
    [InlineData(4, 0b1111_0000, 0b0000_1111)]
    [InlineData(5, 0b1111_1000, 0b0000_0111)]
    [InlineData(6, 0b1111_1100, 0b0000_0011)]
    [InlineData(7, 0b1111_1110, 0b0000_0001)]
    [InlineData(8, 0b1111_1111, 0b0000_0000)]
    public void ByteSplicerTest(int highBits, byte onesHigh, byte onesLow)
    {
        var sut = new ByteSplicer(highBits);
        Assert.Equal(onesHigh, sut.Splice(0xFF, 0));
        Assert.Equal(onesLow, sut.Splice(0, 0xFF));
        
    }
    [Theory]
    [InlineData(0,0,32, "00001111 11111111 11111111 11110000")]
    [InlineData(0,0,16, "00001111 11111111")]
    [InlineData(0,0,27, "00001111 11111111 11111111 11100000")]
    [InlineData(0,0,26, "00001111 11111111 11111111 11000000")]
    [InlineData(0,0,25, "00001111 11111111 11111111 10000000")]
    [InlineData(0,0,24, "00001111 11111111 11111111")]
    [InlineData(4,0,16, "11111111 11111111")]
    [InlineData(4,0,24, "11111111 11111111 11111111")]
    [InlineData(4,0,20, "11111111 11111111 11110000")]
    [InlineData(3,0,32, "01111111 11111111 11111111 10000000")] 
    [InlineData(2,0,32, "00111111 11111111 11111111 11000000")] 
    [InlineData(6,6,32, "00000011 11111111 11111111 11110000")]
    [InlineData(7,7,32, "00000001 11111111 11111111 11110000")]
    [InlineData(3,3,32, "00001111 11111111 11111111 11110000")]
    [InlineData(2,4,28, "00000011 11111111 11111111 11111100")]
    [InlineData(4,2,30, "00111111 11111111 11111111 11000000")]
    [InlineData(4,1,30, "01111111 11111111 11111111 10000000")]
    
    public void SimpleBitCopy(byte srcBits, byte destBits, uint bitlen, string result)
    {
        var src = "00001111 11111111 11111111 11110000".BitsFromBinary();
        var dest = result.BitsFromBinary();
        dest.AsSpan().Clear();
        BitCopierFactory.Create(srcBits, destBits, bitlen, CombinationOperator.Replace).Copy(src.AsSpan(), dest.AsSpan());
        Assert.Equal(result.BitsFromBinary(), dest);
        
    }
}