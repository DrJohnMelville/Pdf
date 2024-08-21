using System.Threading.Tasks;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Postscript.Interpreter.Tokenizers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Primatives;

public class PdfObjectTokenizerTest
{
    [Theory]
    [InlineData("true false", "true", "false")]
    [InlineData("true%hello\r\nfalse", "true", "false")]
    public async Task TwoTokenTestAsync(string content, string text1, string text2)
    {
        using var source = MultiplexSourceFactory.Create(content.AsExtendedAsciiBytes());
        using var readPipeFrom = source.ReadPipeFrom(0);
        var parser = new PdfTokenizer(readPipeFrom);

        Assert.Equal(text1, (await parser.NextTokenAsync()).ToString());
        Assert.Equal(text2, (await parser.NextTokenAsync()).ToString());
    }
}