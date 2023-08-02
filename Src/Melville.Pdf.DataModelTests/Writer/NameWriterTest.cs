using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer;

public class NameWriterTest
{
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
    public async Task WriteNameAsync(string printedAs, string nameText)
    {
        var pdfDirectValue = PdfDirectValue.CreateName(nameText);
        Assert.Equal(printedAs, await pdfDirectValue.WriteToStringAsync());

    }
}