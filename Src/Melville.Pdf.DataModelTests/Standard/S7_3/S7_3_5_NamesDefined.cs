using System.Text;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_5_NamesDefined
{
    [Theory]
    [InlineData("Foo")]
    [InlineData("Fo\u1234o")]
    public void NameCanRenderInUtf8(string name)
    {
        Assert.Equal(name, PdfDirectValue.CreateName(name).ToString());
    }
    
    private static async ValueTask<PdfDirectValue> TryParseStringToNameAsync(string source) =>
        await (await Encoding.UTF8.GetBytes(source).ParseValueObjectAsync())
            .LoadValueAsync();

    [Theory]
    [InlineData("/","")]
    [InlineData("/Name1","Name1")]
    [InlineData("/ASomewhatLongerName","ASomewhatLongerName")]
    [InlineData("/A;NameWith-Various***Characters?","A;NameWith-Various***Characters?")]
    [InlineData("/1.2","1.2")]
    [InlineData("/$$","$$")]
    [InlineData("/@pattern","@pattern")]
    [InlineData("/.notdef",".notdef")]
    [InlineData("/Lime#20Green","Lime Green")]
    [InlineData("/Paired#28#29parentheses","Paired()parentheses")]
    [InlineData("/The_Key_of_F#23_Minor","The_Key_of_F#_Minor")]
    [InlineData("/A#42","AB")]
    
    public async Task ParseNameSucceedAsync(string source, string result)
    {
        var name = await TryParseStringToNameAsync(source);
        Assert.True(name.IsName);
        Assert.Equal(result, name!.ToString());

    }

    [Fact]
    public async Task KnowNamesParseToConstantsAsync()
    {
        var n1 = await TryParseStringToNameAsync("/Width");
        var n2 = await TryParseStringToNameAsync("/Width");
        Assert.True(n1.Equals(n2));
    }
}