using System.Security.Cryptography.X509Certificates;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class BinaryBitmapTest
{
    private readonly BinaryBitmap source = ".B.\r\nB.bB\r\n.B.".AsBinaryBitmap(3,3);
    [Theory]
    [InlineData(1,1, ".....\r\n..B..\r\n.B.B.\r\n..B..\r\n.....")]
    [InlineData(0,1, "..B..\r\n.B.B.\r\n..B..\r\n.....\r\n.....")]
    [InlineData(-1,1, "\r\n.B.B.\r\n..B..\r\n.....\r\n.....\r\n.....")]
    public void SimpleCopyTest(int row, int column, string result)
    {
        var sut = new BinaryBitmap(5, 5);
        sut.CopyTo(row, column, source, CombinationOperator.Replace);
        Assert.Equal(result, sut.BitmapString());
        
    }
}