using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Xunit;

namespace Melville.Pdf.DataModelTests.Postscript.Interpreter.Tokenizers;

public class TokenizerTest
{
    [Theory]
    [InlineData("/App1 App2", "/App1", "App2")]
    [InlineData("8#78 +123L", "8#78", "+123L")] // look like numbers but they're not
    [InlineData("37#78 -1E1.2", "37#78", "-1E1.2")] // look like numbers but they're not
    [InlineData("/App1 %This is a comment\r App2", "/App1", "App2")]
    [InlineData("/App1 %This is a comment\r\n App2", "/App1", "App2")]
    [InlineData("/App1 %This is a comment\r\r\r\r\nApp2", "/App1", "App2")]
    [InlineData("[]", "[", "]")]
    [InlineData("{}", "{", "}")]
    [InlineData("{[", "{", "[")]
    [InlineData("<<<<", "<<", "<<")]
    [InlineData("<<>>", "<<", ">>")]
    public async Task Parse2NamesAsync(string source, string name1, string name2)
    {
        var sut = new Tokenizer(source);
        await VerifyTokenAsync(sut, name1);
        await VerifyTokenAsync(sut, name2);
    }

    private static async Task VerifyTokenAsync(Tokenizer sut, string name)
    {
        var token1 = await sut.NextTokenAsync();
        Assert.Equal(name, token1.Get<string>());
        Assert.False(token1.TryGet<double>(out _));
    }

    [Theory]
    [InlineData("<41425b>", "(AB[)")]
    [InlineData("<414250>", "(ABP)")]
    [InlineData("<41425B>", "(AB[)")]
    [InlineData("<41425>", "(ABP)")]
    [InlineData("<>", "()")]
    [InlineData("<    \r\n    >", "()")]
    [InlineData("(He(ll)o)", "(He(ll)o)")]
    public async Task TestStringParseAsync(string source, string result)
    {
        var token = await new Tokenizer(source).NextTokenAsync();
        Assert.Equal(result, token.Get<string>());
    }

    [Theory]
    [InlineData("\0\0\0\0", "z")]
    [InlineData("\0", "!!")]
    [InlineData("\x1", "!<")]
    [InlineData("\x1\x1", "!<E")]
    [InlineData("\x1\x1\x1", "!<E3")]
    [InlineData("\x1\x1\x1\x1", "!<E3%")]
    [InlineData("\xA", "$3")]
    [InlineData("\xA\xA", "$46")]
    [InlineData("\xA\xA\xA", "$47+")]
    [InlineData("\xA\xA\xA\xA", "$47+I")]
    [InlineData("d", "A,")]
    [InlineData("dd", "A7P")]
    [InlineData("ddd", "A7T3")]
    [InlineData("dddd", "A7T4]")]
    [InlineData("\xFF\xFF\xFF\xFF", "s8W-!")]
    [InlineData("dddd\0\0\0\0dddd", "A7T4]zA7T4]")]
    [InlineData("", "")]
    public Task Accii85StringAsync(string decoded, string encoded) =>
        TestStringParseAsync($"<~{encoded}~>", 
            PostscriptValueFactory.CreateString(decoded, StringKind.String).Get<string>());

    [Fact]
    public Task ExceptionForMismatchedCloseWakkaAsync() =>
        Assert.ThrowsAsync<PostscriptParseException>(()=>
            new Tokenizer(">").NextTokenAsync().AsTask()
        );

    [Theory]
    [InlineData("1234", 1234)]
    [InlineData("+1234", 1234)]
    [InlineData("-1234", -1234)]
    [InlineData("16#A", 10)]
    [InlineData("16#Ab9", 0xab9)]
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
    [InlineData("1.25E+2", 125)]
    [InlineData("12500.0e-2", 125)]
    [InlineData("12500e-2", 125)]
    [InlineData("12500E-2", 125)]
    public async Task ParseFloatAsync(string source, float result)
    {
        var sut = new Tokenizer(source);
        var token = await sut.NextTokenAsync();
        Assert.Equal(result, token.Get<double>(), 4);
    }
}