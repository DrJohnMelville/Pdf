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
    // from standard section 7.3.3 example 1
    [InlineData("123", 123)]
    [InlineData("43455", 43455)]
    [InlineData("+17", 17)]
    [InlineData("-98", -98)]
    [InlineData("0", 0)]
    public Task SpecExample1ItemsAsync(string input, int value) => ParseNumberSucceedAsync(input, value, value);

    [Theory]
    // from standard section 7.3.3 example 2
    [InlineData("34.5", 34.5)]
    [InlineData("-3.62", -3.62)]
    [InlineData("+123.6", 123.6)]
    [InlineData("4.", 4)]
    [InlineData("-.002", -0.002)]
    public Task SpecExample2ItemsAsync(string input, double value) => ParseNumberSucceedAsync(input, (int)value, value);
    
    
    [Theory]
    [InlineData("%comment\r\n34/", 34, 34)]
    // from standard sec
    public async Task ParseNumberSucceedAsync(string source, int intValue, double doubleValue)
    {
        var num = (PdfNumber)await source.ParseObjectAsync(); 
        Assert.Equal(intValue, num!.IntValue);
        Assert.Equal(doubleValue, num.DoubleValue);
    }
}