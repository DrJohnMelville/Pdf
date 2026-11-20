using System.Text;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.StreamInterfaces.Memory;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_4_StringsDefined
{
    [Test]
    public void StringObjectDefinitions()
    {
        var str = PdfDirectObject.CreateString("Foo Bar"u8);
        Assert.Equal("Foo Bar", str.ToString());
    }

    [Test]
    [Arguments("<>/", "")]
    [Arguments("<20>/", " ")]
    [Arguments("<2020>/", "  ")]
    [Arguments("<202>/", "  ")] // last byte is implicit 0 because it is missing
    [Arguments("<01234567ABCDEF>/", "\x01\x23\x45\x67\xAB\xCD\xEF")]
    [Arguments("<01 23  4 \r\n567   ABC \tDEF>/", "\x01\x23\x45\x67\xAB\xCD\xEF")]
    public async Task ParseHexStringAsync(string input, string output)
    {
        var str = await (await input.ParseValueObjectAsync()).LoadValueAsync();
        Assert.Equal(output, 
            ExtendedAsciiEncoding.ExtendedAsciiString(str.Get<StringSpanSource>().GetSpan()));
    }

    [Test]
    [Arguments("()/", "")]
    [Arguments("(Hello)/", "Hello")]
    [Arguments("(He(l(lo)))/", "He(l(lo))")] // balanced parens are legal.
    [Arguments("(\\n)", "\n")]
    [Arguments("(\\r)", "\r")]
    [Arguments("(\\t)", "\t")]
    [Arguments("(\\b)", "\b")]
    [Arguments("(\\f)", "\f")]
    [Arguments("(\\()", "(")]
    [Arguments("(\\))", ")")]
    [Arguments("(\\\\))", "\\")]

    //reverse solidus works as a line continuation character
    [Arguments("(a\\\r\nb))", "ab")]
    [Arguments("(a\\\nb))", "ab")]
    [Arguments("(a\\\rb))", "ab")]
    
    // All line breaks in a string represent as \n no mattar how they appear in the pdf file
    [Arguments("(\r\n\\\\))", "\n\\")]  // a \r\n internal to a string reads as \n
    [Arguments("(a\r\nb))", "a\nb")]
    [Arguments("(a\rb))", "a\nb")]
    [Arguments("(a\nb))", "a\nb")]
    [Arguments("(a\n\nb))", "a\n\nb")]
    [Arguments("(a\r\n\nb))", "a\n\nb")]
    [Arguments("(a\r\n\r\nb))", "a\n\nb")]
    
    // strings using octal escapes
    [Arguments("(a\\1b))", "a\x0001b")]
    [Arguments("(a\\21b))", "a\x0011b")]
    [Arguments("(a\\121b))", "a\x0051b")]
    [Arguments("(a\\1212b))", "a\x00512b")]
    public async Task ParseLiteralStringAsync(string source, string result)
    {
        var parsedString = await (await source.ParseValueObjectAsync()).LoadValueAsync();
        Assert.True(parsedString.IsString);
        Assert.Equal(result, parsedString.ToString());
    }

    [Test]
    [Arguments("","", 0)]
    [Arguments("Hello","Hello", 0)]
    [Arguments("a","", 1)]
    [Arguments("aaab","aaaa", 1)]
    [Arguments("a","aa", -1)]
    [Arguments("azzzz","baaaa", -1)]
    public void CompareStrings(string a, string b, int result)
    {
        Assert.Equal(result, PdfDirectObject.CreateString(a.AsExtendedAsciiBytes())
            .CompareTo(PdfDirectObject.CreateString(b.AsExtendedAsciiBytes())));
        Assert.Equal(result*-1, PdfDirectObject.CreateString(b.AsExtendedAsciiBytes())
            .CompareTo(PdfDirectObject.CreateString(a.AsExtendedAsciiBytes())));
    }
}