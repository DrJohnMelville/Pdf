using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
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
        var fact = new BitmapTemplateFactory(SymbolDictionaryTemplate.V1);
        fact.AddPoint(row, col);
        var temolplate = fact.Create();
        Assert.Equal(result, temolplate.ReadContext(bitmap, 2,3));
    }
}

