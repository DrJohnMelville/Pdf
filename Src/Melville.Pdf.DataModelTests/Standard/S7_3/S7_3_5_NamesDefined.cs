using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_5_NamesDefined
{
    [Test]
    [Arguments("Foo")]
    [Arguments("Fo\u1234o")]
    public void NameCanRenderInUtf8(string name)
    {
        Assert.Equal(name, PdfDirectObject.CreateName(name).ToString());
    }
    
    private static async ValueTask<PdfDirectObject> TryParseStringToNameAsync(string source) =>
        await (await Encoding.UTF8.GetBytes(source).ParseValueObjectAsync())
            .LoadValueAsync();

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
    [Arguments("/A#42","AB")]
    
    public async Task ParseNameSucceedAsync(string source, string result)
    {
        var name = await TryParseStringToNameAsync(source);
        Assert.True(name.IsName);
        Assert.Equal(result, name!.ToString());

    }

    [Test]
    public async Task KnowNamesParseToConstantsAsync()
    {
        var n1 = await TryParseStringToNameAsync("/Width");
        var n2 = await TryParseStringToNameAsync("/Width");
        Assert.True(n1.Equals(n2));
    }

    [Test]
    public void KnownameIsCorrect()
    {
        Assert.Equal("Page", KnownNames.Page);
        Assert.Equal("Helvetica-Bold", KnownNames.HelveticaBold);
        Assert.Equal("Helvetica-BoldOblique", KnownNames.HelveticaBoldOblique);
    }
}