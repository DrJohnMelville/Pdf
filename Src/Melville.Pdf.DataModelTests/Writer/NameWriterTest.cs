using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.DataModelTests.Writer;

public class NameWriterTest
{
    [Test]
    [Arguments("/","")]
    [Arguments("/Name1","Name1")]
    [Arguments("/ASomewhatLongerName","ASomewhatLongerName")]
    [Arguments("/A;NameWith-Various***Characters?","A;NameWith-Various***Characters?")]
    [Arguments("/1.2","1.2")]
    [Arguments("/$$","$$")]
    [Arguments("/@pattern","@pattern")]
    [Arguments("/.notdef",".notdef")]
    [Arguments("/Lime#20Green","Lime Green")]
    [Arguments("/Paired#28#29parentheses","Paired()parentheses")]
    [Arguments("/The_Key_of_F#23_Minor","The_Key_of_F#_Minor")]
    public async Task WriteNameAsync(string printedAs, string nameText)
    {
        var pdfDirectValue = PdfDirectObject.CreateName(nameText);
        Assert.Equal(printedAs, await pdfDirectValue.WriteToStringAsync());

    }
}