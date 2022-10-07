using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Conventions;
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
        Assert.Equal("/" + name, new PdfName(name).ToString());
    }

    [Theory]
    [InlineData("Foo", "Foo", true)]
    [InlineData("Foo", "Bar", false)]
    [InlineData("Foo", "Foot", false)]
    public void EqualityTest(string a, string b, bool matches)
    {
        var nameA = new PdfName(a);
        var nameB = new PdfName(b);
        Assert.Equal(matches, nameA.Equals(nameB));
        Assert.Equal(matches, (object)nameA.Equals(nameB));
        Assert.Equal(matches, nameA.GetHashCode() == nameB.GetHashCode());

    }


    private static async Task<PdfName> TryParseStringToName(string source)
    {
        return (PdfName)await Encoding.UTF8.GetBytes(source).ParseObjectAsync();
    }

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
    
    public async Task ParseNameSucceed(string source, string result)
    {
        var name = await TryParseStringToName(source);
        Assert.Equal("/" + result, name!.ToString());

    }

    [Fact]
    public async Task KnowNamesParseToConstants()
    {
        var n1 = await TryParseStringToName("/Width");
        var n2 = await TryParseStringToName("/Width");
        Assert.True(ReferenceEquals(KnownNames.Width, n1));
        Assert.True(ReferenceEquals(n1, n2));
    }
}