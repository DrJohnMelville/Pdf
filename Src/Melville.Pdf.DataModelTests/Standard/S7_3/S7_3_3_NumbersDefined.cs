using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_3_NumbersDefined
{
    [Theory]
    [InlineData("1/", 1, 1)]
    [InlineData("6/", 6, 6)]
    [InlineData("34/", 34, 34)]
    [InlineData("%comment\r\n34/", 34, 34)]
    [InlineData("+34/", 34, 34)]
    [InlineData("-34/", -34, -34)]
    [InlineData("-34./", -34, -34)]
    [InlineData("134.567/", 134, 134.567)]
    [InlineData("134.567", 134, 134.567)]
    [InlineData("134", 134, 134)]
    public async Task ParseNumberSucceed(string source, int intValue, double doubleValue)
    {
        var num = (PdfNumber)await source.ParseObjectAsync(); 
        Assert.Equal(intValue, num!.IntValue);
        Assert.Equal(doubleValue, num.DoubleValue);
    }
}