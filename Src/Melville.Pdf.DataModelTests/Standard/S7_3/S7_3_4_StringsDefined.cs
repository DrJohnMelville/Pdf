using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Postscript.Interpreter.Values;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_4_StringsDefined
{
    [Fact]
    public void StringObjectDefinitions()
    {
        var str = PdfDirectValue.CreateString("Foo Bar"u8);
        Assert.Equal("Foo Bar", str.ToString());
    }

    [Theory]
    [InlineData("<>/", "")]
    [InlineData("<20>/", " ")]
    [InlineData("<2020>/", "  ")]
    [InlineData("<202>/", "  ")] // last byte is implicit 0 because it is missing
    [InlineData("<01234567ABCDEF>/", "\x01\x23\x45\x67\xAB\xCD\xEF")]
    [InlineData("<01 23  4 \r\n567   ABC \tDEF>/", "\x01\x23\x45\x67\xAB\xCD\xEF")]
    public async Task ParseHexStringAsync(string input, string output)
    {
        var str = await (await input.ParseValueObjectAsync()).LoadValueAsync();
        Assert.Equal(output, 
            ExtendedAsciiEncoding.ExtendedAsciiString(str.Get<StringSpanSource>().GetSpan()));
    }

    [Theory]
    [InlineData("()/", "")]
    [InlineData("(Hello)/", "Hello")]
    [InlineData("(He(l(lo)))/", "He(l(lo))")] // balanced parens are legal.
    [InlineData("(\\n)", "\n")]
    [InlineData("(\\r)", "\r")]
    [InlineData("(\\t)", "\t")]
    [InlineData("(\\b)", "\b")]
    [InlineData("(\\f)", "\f")]
    [InlineData("(\\()", "(")]
    [InlineData("(\\))", ")")]
    [InlineData("(\\\\))", "\\")]

    //reverse solidus works as a line continuation character
    [InlineData("(a\\\r\nb))", "ab")]
    [InlineData("(a\\\nb))", "ab")]
    [InlineData("(a\\\rb))", "ab")]
    
    // All line breaks in a string represent as \n no mattar how they appear in the pdf file
    [InlineData("(\r\n\\\\))", "\n\\")]  // a \r\n internal to a string reads as \n
    [InlineData("(a\r\nb))", "a\nb")]
    [InlineData("(a\rb))", "a\nb")]
    [InlineData("(a\nb))", "a\nb")]
    [InlineData("(a\n\nb))", "a\n\nb")]
    [InlineData("(a\r\n\nb))", "a\n\nb")]
    [InlineData("(a\r\n\r\nb))", "a\n\nb")]
    
    // strings using octal escapes
    [InlineData("(a\\1b))", "a\x0001b")]
    [InlineData("(a\\21b))", "a\x0011b")]
    [InlineData("(a\\121b))", "a\x0051b")]
    [InlineData("(a\\1212b))", "a\x00512b")]
    public async Task ParseLiteralStringAsync(string source, string result)
    {
        var parsedString = await (await source.ParseValueObjectAsync()).LoadValueAsync();
        Assert.True(parsedString.IsString);
        Assert.Equal(result, parsedString.ToString());
    }

    [Theory]
    [InlineData("","", 0)]
    [InlineData("Hello","Hello", 0)]
    [InlineData("a","", 1)]
    [InlineData("aaab","aaaa", 1)]
    [InlineData("a","aa", -1)]
    [InlineData("azzzz","baaaa", -1)]
    public void CompareStrings(string a, string b, int result)
    {
        Assert.Equal(result, PdfDirectValue.CreateString(a.AsExtendedAsciiBytes())
            .CompareTo(PdfDirectValue.CreateString(b.AsExtendedAsciiBytes())));
        Assert.Equal(result*-1, PdfDirectValue.CreateString(b.AsExtendedAsciiBytes())
            .CompareTo(PdfDirectValue.CreateString(a.AsExtendedAsciiBytes())));
    }
}