using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard._7
{
    public class S7_3_3_NumbersDefined
    {
        [Theory]
        [InlineData("1/", 1, 1)]
        [InlineData("6/", 6, 6)]
        [InlineData("34/", 34, 34)]
        [InlineData("+34/", 34, 34)]
        [InlineData("-34/", -34, -34)]
        [InlineData("-34./", -34, -34)]
        [InlineData("134.567/", 134, 134.567)]
        public void ParseNumberSucceed(string source, int intValue, double doubleValue)
        {
            Assert.True(source.ParseAs(out PdfNumber? num));
            Assert.Equal(intValue, num!.IntValue);
            Assert.Equal(doubleValue, num.DoubleValue);
        }

        [Theory]
        [InlineData("")]
        [InlineData("1")]
        [InlineData("12345")]
        [InlineData("12345.")]
        [InlineData("12345.09374")]
        [InlineData("12345.09374 ")]
        public void ParseNumberIncomplete(string num)
        {
            Assert.False(num.ParseAs());
        }
    
    }
}