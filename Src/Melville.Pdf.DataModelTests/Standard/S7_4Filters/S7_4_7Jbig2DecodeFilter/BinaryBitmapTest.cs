using System.Security.Cryptography.X509Certificates;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class BinaryBitmapTest
{
    [Theory]
    [InlineData(1,1, ".....\r\n..B..\r\n.B.B.\r\n..B..\r\n.....")]
    [InlineData(0,1, "..B..\r\n.B.B.\r\n..B..\r\n.....\r\n.....")]
    [InlineData(-1,1, ".B.B.\r\n..B..\r\n.....\r\n.....\r\n.....")]
    [InlineData(2,1, ".....\r\n.....\r\n..B..\r\n.B.B.\r\n..B..")]
    [InlineData(3,1, ".....\r\n.....\r\n.....\r\n..B..\r\n.B.B.")]
    [InlineData(1,0, ".....\r\n.B...\r\nB.B..\r\n.B...\r\n.....")]
    [InlineData(1,-1, ".....\r\nB....\r\n.B...\r\nB....\r\n.....")]
    [InlineData(1,2, ".....\r\n...B.\r\n..B.B\r\n...B.\r\n.....")]
    [InlineData(1,3, ".....\r\n....B\r\n...B.\r\n....B\r\n.....")]
    public void SimpleCopyTest(int row, int column, string result)
    {
        var source = ".B.\r\nB.bB\r\n.B.".AsBinaryBitmap(3,3);
        var sut = new BinaryBitmap(5, 5);
        sut.PasteBitsFrom(row, column, source, CombinationOperator.Replace);
        Assert.Equal(result, sut.BitmapString());
        
    }

    [Theory]
    [InlineData(CombinationOperator.Replace, ".B.B")]
    [InlineData(CombinationOperator.And, "...B")]
    [InlineData(CombinationOperator.Or, ".BBB")]
    [InlineData(CombinationOperator.Xor, ".BB.")]
    [InlineData(CombinationOperator.Xnor, "B..B")]
    public void CombinationTest(CombinationOperator op, string result)
    {
        var a = "..BB".AsBinaryBitmap(1,4);
        var b = ".B.B".AsBinaryBitmap(1,4);
        a.PasteBitsFrom(0,0, b, op);
        Assert.Equal(result, a.BitmapString());
        
    }
}