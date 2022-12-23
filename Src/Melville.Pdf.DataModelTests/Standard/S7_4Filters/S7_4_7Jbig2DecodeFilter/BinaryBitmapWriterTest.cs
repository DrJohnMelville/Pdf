using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.SegmentParsers.TextRegions;
using Melville.JBig2.Segments;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class BinaryBitmapWriterTest
{
    private readonly Mock<IBitmapCopyTarget> target = new();
    private readonly Mock<IBinaryBitmap> source = new();

    public BinaryBitmapWriterTest()
    {
        target.SetupGet(i => i.Width).Returns(50);
        target.SetupGet(i => i.Height).Returns(70);
        source.SetupGet(i => i.Width).Returns(5);
        source.SetupGet(i => i.Height).Returns(7);
    }

    [Theory]
    [InlineData(ReferenceCorner.TopLeft, false, 13,10, 14)]
    [InlineData(ReferenceCorner.TopRight, false, 13,10, 14)]
    [InlineData(ReferenceCorner.BottomLeft, false, 7, 10, 14)]
    [InlineData(ReferenceCorner.BottomRight, false, 7, 10, 14)]

    [InlineData(ReferenceCorner.TopLeft, true, 10, 13, 16)]
    [InlineData(ReferenceCorner.BottomLeft, true, 10, 13, 16)]
    [InlineData(ReferenceCorner.TopRight, true, 10, 9, 16)]
    [InlineData(ReferenceCorner.BottomRight, true, 10, 9, 16)]
    public void DisplayCornerTest(object corner, bool transposed, int row, int column, int finalS)
    {
        var sut = new BinaryBitmapWriter(target.Object, transposed, (ReferenceCorner)corner, CombinationOperator.Or);
        var s = 10;
        sut.WriteBitmap(13, ref s, source.Object);
        target.Verify(i=>i.PasteBitsFrom(row, column, source.Object, CombinationOperator.Or));
        Assert.Equal(finalS, s);
        
    }
    [Theory]
    [InlineData(CombinationOperator.Replace)]
    [InlineData(CombinationOperator.And)]
    public void BitOperationTest(object op)
    {
        var sut = new BinaryBitmapWriter(target.Object, false, ReferenceCorner.TopLeft, (CombinationOperator)op);
        var s = 10;
        sut.WriteBitmap(13, ref s, source.Object);
        target.Verify(i=>i.PasteBitsFrom(It.IsAny<int>(), It.IsAny<int>(), source.Object, (CombinationOperator)op));
    }
}