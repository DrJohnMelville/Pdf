using System;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.Segments;
using Melville.Pdf.ReferenceDocuments.Utility;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class BinaryBitmapTest
{
    [Test]
    [Arguments(1,1, ".....\r\n..B..\r\n.B.B.\r\n..B..\r\n.....")]
    [Arguments(0,1, "..B..\r\n.B.B.\r\n..B..\r\n.....\r\n.....")]
    [Arguments(-1,1, ".B.B.\r\n..B..\r\n.....\r\n.....\r\n.....")]
    [Arguments(2,1, ".....\r\n.....\r\n..B..\r\n.B.B.\r\n..B..")]
    [Arguments(3,1, ".....\r\n.....\r\n.....\r\n..B..\r\n.B.B.")]
    [Arguments(1,0, ".....\r\n.B...\r\nB.B..\r\n.B...\r\n.....")]
    [Arguments(1,-1, ".....\r\nB....\r\n.B...\r\nB....\r\n.....")]
    [Arguments(1,2, ".....\r\n...B.\r\n..B.B\r\n...B.\r\n.....")]
    [Arguments(1,3, ".....\r\n....B\r\n...B.\r\n....B\r\n.....")]
    public void SimpleCopyTest(int row, int column, string result)
    {
        var source = ".B.\r\nB.B\r\n.B.".AsBinaryBitmap(3,3);
        var sut = new BinaryBitmap(5, 5);
        sut.PasteBitsFrom(row, column, source, CombinationOperator.Replace);
        Assert.Equal(result, sut.BitmapString());
        
    }

    [Test]
    [Arguments(CombinationOperator.Replace, ".B.B")]
    [Arguments(CombinationOperator.And, "...B")]
    [Arguments(CombinationOperator.Or, ".BBB")]
    [Arguments(CombinationOperator.Xor, ".BB.")]
    [Arguments(CombinationOperator.Xnor, "B..B")]
    public void CombinationTest(object op, string result)
    {
        var a = "..BB".AsBinaryBitmap(1,4);
        var b = ".B.B".AsBinaryBitmap(1,4);
        a.PasteBitsFrom(0,0, b, (CombinationOperator)op);
        Assert.Equal(result, a.BitmapString());
        
    }
    
        [Test]
    [Arguments(0, 0b0000_0000, 0b1111_1111)]
    [Arguments(1, 0b1000_0000, 0b0111_1111)]
    [Arguments(2, 0b1100_0000, 0b0011_1111)]
    [Arguments(3, 0b1110_0000, 0b0001_1111)]
    [Arguments(4, 0b1111_0000, 0b0000_1111)]
    [Arguments(5, 0b1111_1000, 0b0000_0111)]
    [Arguments(6, 0b1111_1100, 0b0000_0011)]
    [Arguments(7, 0b1111_1110, 0b0000_0001)]
    [Arguments(8, 0b1111_1111, 0b0000_0000)]
    public void ByteSplicerTest(int highBits, byte onesHigh, byte onesLow)
    {
        var sut = new ByteSplicer(highBits);
        Assert.Equal(onesHigh, sut.Splice(0xFF, 0));
        Assert.Equal(onesLow, sut.Splice(0, 0xFF));
        
    }
    [Test]
    [Arguments(0,0,32, "00001111 11111111 11111111 11110000")]
    [Arguments(0,0,16, "00001111 11111111")]
    [Arguments(0,0,27, "00001111 11111111 11111111 11100000")]
    [Arguments(0,0,26, "00001111 11111111 11111111 11000000")]
    [Arguments(0,0,25, "00001111 11111111 11111111 10000000")]
    [Arguments(0,0,24, "00001111 11111111 11111111")]
    [Arguments(4,0,16, "11111111 11111111")]
    [Arguments(4,0,24, "11111111 11111111 11111111")]
    [Arguments(4,0,20, "11111111 11111111 11110000")]
    [Arguments(3,0,32, "01111111 11111111 11111111 10000000")] 
    [Arguments(2,0,32, "00111111 11111111 11111111 11000000")] 
    [Arguments(6,6,32, "00000011 11111111 11111111 11110000")]
    [Arguments(7,7,32, "00000001 11111111 11111111 11110000")]
    [Arguments(3,3,32, "00001111 11111111 11111111 11110000")]
    [Arguments(2,4,28, "00000011 11111111 11111111 11111100")]
    [Arguments(4,2,30, "00111111 11111111 11111111 11000000")]
    [Arguments(4,1,30, "01111111 11111111 11111111 10000000")]
    
    public void SimpleBitCopy(byte srcBits, byte destBits, int bitlen, string result)
    {
        var src = "00001111 11111111 11111111 11110000".BitsFromBinary();
        var dest = result.BitsFromBinary();
        dest.AsSpan().Clear();
        new BitCopierFactory(srcBits, destBits, bitlen, CombinationOperator.Replace)
            .Create()
            .Copy(src.AsSpan(), dest.AsSpan());
        Assert.SeqEqual(result.BitsFromBinary(), dest);
        
    }
    
    [Test]
    [Arguments(0,0,4, "BBBB............")]
    [Arguments(1,0,4, "BBBB............")]
    [Arguments(7,0,4, "BBBB............")]
    [Arguments(0,1,4, ".BBBB...........")]
    [Arguments(1,1,4, ".BBBB...........")]
    [Arguments(0,1,6, ".BBBBBB.........")]
    [Arguments(0,1,7, ".BBBBBBB........")]
    [Arguments(0,6,4, "......BBBB......")]
    [Arguments(3,6,4, "......BBBB......")]
    [Arguments(7,6,4, "......BBBB......")]
    public void ShortCopy(int srcCol, int destCol, int length, string result)
    {
        var source = "BBBBBBBB BBBBBBBB".AsBinaryBitmap(1, 16);
        var extSource = new OffsetBitmap(source, 0, srcCol, 1, length);
        var dest = new BinaryBitmap(1, 16);
        dest.PasteBitsFrom(0, destCol, extSource, CombinationOperator.Replace);
        Assert.Equal(result, dest.BitmapString());
    }

    [Test]
    [Arguments(0,0,4, "BBBB............")]
    [Arguments(1,0,4, "BBBB............")]
    [Arguments(7,0,4, "B...............")]
    [Arguments(0,1,4, ".BBBB...........")]
    [Arguments(1,1,4, ".BBBB...........")]
    [Arguments(0,1,6, ".BBBBBB.........")]
    [Arguments(0,1,7, ".BBBBBBB........")]
    [Arguments(0,6,4, "......BBBB......")]
    [Arguments(3,6,4, "......BBBB......")]
    [Arguments(7,6,4, "......B.........")]
    [Arguments(6,7,4, ".......BB.......")]
    // need some cases where 2 bits of CodeSource go into one bit of dest
    public void ShortCopy2(int srcCol, int destCol, int length, string result)
    {
        var source = "BBBBBBBB ........".AsBinaryBitmap(1, 16);
        var extSource = new OffsetBitmap(source, 0, srcCol, 1, length);
        var dest = new BinaryBitmap(1, 16);
        dest.PasteBitsFrom(0, destCol, extSource, CombinationOperator.Replace);
        Assert.Equal(result, dest.BitmapString());
    }

    [Test]
    [Arguments("B.B..B.....B...B", 7, 4, 5, "........B.......")]
    [Arguments("BBBB.B...B..B..B", 5, 5, 7, ".....B...B......")]
    public void BitmapCopyBugs(string sourceString, int srcPos, int destPos, int length, string result)
    {
        var source = sourceString.AsBinaryBitmap(1,16);
        var finalSource = new OffsetBitmap(source, 0, srcPos, 1, length);

        var fastDest = new BinaryBitmap(1, 16);
        fastDest.PasteBitsFrom(0, destPos, finalSource, CombinationOperator.Replace);
        Assert.Equal(result, fastDest.BitmapString());
        
    }
}