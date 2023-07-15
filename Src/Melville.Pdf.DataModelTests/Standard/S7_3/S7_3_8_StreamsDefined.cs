using System.IO;
using System.Threading.Tasks;
using Melville.Hacks.Reflection;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_8_StreamsDefined
{
    private static long GetPosition(PdfStream obj) => 
        (long)(obj.GetField("source")!.GetField("sourceFilePosition")!);

    private const string ObjectPrefix = "1 2 obj ";
    [Theory]
    [InlineData("<</Length 6>> stream\r\n123456\r\nendstream", 22)]
    [InlineData("<</Length 6>> stream\n123456\r\nendstream", 21)]
    // PDF Spec section 7.3.8.1 says this is illegal but real pdf files do it, and PDF reader accepts it.
    [InlineData("<</Length 6>> stream\r123456\r\nendstream", 21)]
    [InlineData("                                    <</Length 6>> stream\r123456\r\nendstream", 21)]
    public async Task ParseSimpleStreamAsync(string data, int expectedPosition)
    {
        var obj = await $"{ObjectPrefix}{data}\r\nendobj".ParseRootObjectAsync();
        var cSharpStream = await obj.Get<PdfValueStream>().StreamContentAsync();
        Assert.Equal("123456", await new StreamReader(cSharpStream).ReadToEndAsync());
    }
}