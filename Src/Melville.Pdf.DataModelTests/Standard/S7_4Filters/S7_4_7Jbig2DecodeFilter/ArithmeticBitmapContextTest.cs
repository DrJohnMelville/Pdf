using Melville.JBig2.ArithmeticEncodings;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class ArithmeticBitmapContextTest
{
    [Theory]
    [InlineData(-1,3,  0b0101_111111_000)]
    [InlineData(-1,-3, 0b0101_111111_000)]
    [InlineData(-1,1, 0b0101_11111_000)]
    [InlineData(-1,-23, 0b0101_11111_000_0)]
    [InlineData(-1,23, 0b0101_11111_000_0)]
    [InlineData(-1,4, 0b0101_11111_000_1)]
    public void Type1TemplateTest(sbyte row, sbyte col, ushort result)
    {
        var bitmap = @"
.B.B.B.B.B.B.B.B    
BBBBBBBBBBBBBBBB
................".AsBinaryBitmap(3, 16);
        var fact = new BitmapTemplateFactory(GenericRegionTemplate.GB1);
        fact.AddPoint(row, col);
        var temolplate = fact.Create();
        var ic = temolplate.ToIncrementalTemplate();
        ic.SetToPosition(bitmap,2,3);
        Assert.Equal(result, ic.context);
    }
}

