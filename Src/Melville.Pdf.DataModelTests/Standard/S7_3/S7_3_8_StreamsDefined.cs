using System.Threading.Tasks;
using Melville.Hacks.Reflection;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_8_StreamsDefined
{
    private static long GetPosition(PdfStream obj) => 
        (long)(obj.GetField("source")!.GetField("sourceFilePosition")!);

    [Theory]
    [InlineData("<</LENGTH 6>> stream\r\n123456\r\nendstream", 22)]
    [InlineData("<</LENGTH 6>> stream\n123456\r\nendstream", 21)]
    // PDF Spec section 7.3.8.1 says this is illegal but real pdf files do it, and PDF reader accepts it.
    [InlineData("<</LENGTH 6>> stream\r123456\r\nendstream", 21)]
    public async Task ParseSimpleStream(string data, int expectedPosition)
    {
        var obj = (PdfStream)await data.ParseObjectAsync();
        Assert.Equal(expectedPosition, GetPosition(obj));
    }

    [Fact]
    public async Task ParseSimpleStreamAfterJump()
    {
        var parser =  "          <</LENGTH 6>> stream\r\n123456\r\nendstream".AsParsingSource();
        var obj = (PdfStream)await parser.ParseObjectAsync();
        Assert.Equal(32, GetPosition(obj));
        var obj2 =  (PdfStream) await parser.ParseObjectAsync(5);
        Assert.Equal(32, GetPosition(obj2));
    }        
    [Fact]
    public async Task ParseStreamWithMissingData()
    {
        var obj = (PdfStream)await "<</LENGTH 6>> stream\r\n".ParseObjectAsync();
        Assert.Equal(22, GetPosition(obj));
    }        
}