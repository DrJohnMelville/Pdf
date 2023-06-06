using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Postscript.Interpreter.Tokenizers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Postscript.Interpreter.Tokenizers;

public class TokenizerTest
{
    [Theory]
    [InlineData("1234", 1234)]
    [InlineData("+1234", 1234)]
    [InlineData("-1234", -1234)]
    [InlineData("16#A", 10)]
    [InlineData("16#Ab9", 0xab9)]
    [InlineData("8#78", 0x7)]
    [InlineData("2#1001001", 0b1001001)]
    public async Task ParseIntsAsync(string source, int result)
    {
        var sut = new Tokenizer(source);
        var token = await sut.NextTokenAsync();
        Assert.Equal(result, token.Get<int>());
    }

    [Theory]
    [InlineData("1.5", 1.5)]
    [InlineData("-1.5", -1.5)]
    [InlineData("-.005", -0.005)]
    [InlineData("210.", 210)]
    [InlineData("1.25e2", 125)]
    [InlineData("1.25e+2", 125)]
    [InlineData("12500.0e-2", 125)]
    [InlineData("12500e-2", 125)]
    public async Task ParseFloatAsync(string source, float result)
    {
        var sut = new Tokenizer(source);
        var token = await sut.NextTokenAsync();
        Assert.Equal(result, token.Get<double>(), 4);
    }
}