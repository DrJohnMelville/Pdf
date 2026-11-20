using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melville.Parsing.ObjectRentals;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;
using Melville.StreamInterfaces.Memory;

namespace Melville.Pdf.DataModelTests.Postscript.Interpreter.Tokenizers;

public class TokenizerTest
{
    [Test]
    [Arguments("/App1 App2", "/App1", "App2")]
    [Arguments("8#78 +123L", "8#78", "+123L")] // look like numbers but they're not
    [Arguments("37#78 -1E1.2", "37#78", "-1E1.2")] // look like numbers but they're not
    [Arguments("/App1 %This is a comment\r App2", "/App1", "App2")]
    [Arguments("/App1 %This is a comment\r\n App2", "/App1", "App2")]
    [Arguments("/App1 %This is a comment\r\r\r\r\nApp2", "/App1", "App2")]
    [Arguments("[]", "[", "]")]
    [Arguments("{}", "{", "}")]
    [Arguments("{[", "{", "[")]
    [Arguments("<<<<", "<<", "<<")]
    [Arguments("<<>>", "<<", ">>")]
    [Arguments("exch pop", "exch", "pop")]
    public async Task Parse2NamesAsync(string source, string name1, string name2)
    {
        using var sut = CreateTokenizer(source);
        var list = await sut.TokensAsync().ToListAsync();
        Assert.Equal(2, list.Count);
        VerifyToken(list[0], name1);
        VerifyToken(list[1], name2);
    }

    [Test]
    public async Task ParseAsyncWithSyncTokenizerAsync()
    {
        using var sut = CreateSyncTokenizer("/App1 %This is a comment\r\r\r\r\nApp2");
        var list = await sut.TokensAsync().ToListAsync();
        Assert.Equal(2, list.Count);
        VerifyToken(list[0], "/App1");
        VerifyToken(list[1], "App2");

}


private static void VerifyToken(PostscriptValue token1, string name)
    {
        Assert.Equal(name, token1.ToString());
        Assert.False(token1.TryGet<bool>(out _));
    }

    [Test]
    [Arguments("<41425b>", "(AB[)")]
    [Arguments("<414250>", "(ABP)")]
    [Arguments("<41425B>", "(AB[)")]
    [Arguments("<41425>", "(ABP)")]
    [Arguments("<>", "()")]
    [Arguments("<    \r\n    >", "()")]
    [Arguments("(He(ll)o)", "(He(ll)o)")]
    public async Task TestStringParseAsync(string source, string result)
    {
        using var tokenizer = CreateTokenizer(source);
        var token = await tokenizer.TokensAsync().FirstAsync();
        Assert.Equal(result, token.ToString());
    }

    [Test]
    [Arguments("\0\0\0\0", "z")]
    [Arguments("\0", "!!")]
    [Arguments("\x1", "!<")]
    [Arguments("\x1\x1", "!<E")]
    [Arguments("\x1\x1\x1", "!<E3")]
    [Arguments("\x1\x1\x1\x1", "!<E3%")]
    [Arguments("\xA", "$3")]
    [Arguments("\xA\xA", "$46")]
    [Arguments("\xA\xA\xA", "$47+")]
    [Arguments("\xA\xA\xA\xA", "$47+I")]
    [Arguments("d", "A,")]
    [Arguments("dd", "A7P")]
    [Arguments("ddd", "A7T3")]
    [Arguments("dddd", "A7T4]")]
    [Arguments("\xFF\xFF\xFF\xFF", "s8W-!")]
    [Arguments("dddd\0\0\0\0dddd", "A7T4]zA7T4]")]
    [Arguments("", "")]
    public Task Accii85StringAsync(string decoded, string encoded) =>
        TestStringParseAsync($"<~{encoded}~>", 
            PostscriptValueFactory.CreateString(decoded, StringKind.String).ToString());

    [Test]
    public Task ExceptionForMismatchedCloseWakkaAsync() =>
        Assert.ThrowsAsync<PostscriptNamedErrorException>(()=>
        {
            using var tokenizer = CreateTokenizer(">");
            return tokenizer.TokensAsync().FirstAsync().AsTask();
        });

    [Test]
    [Arguments("1234", 1234)]
    [Arguments("+1234", 1234)]
    [Arguments("-1234", -1234)]
    [Arguments("16#A", 10)]
    [Arguments("16#Ab9", 0xab9)]
    [Arguments("2#1001001", 0b1001001)]
    public async Task ParseIntsAsync(string source, int result)
    {
        using var sut = CreateTokenizer(source);
        var token = await sut.TokensAsync().FirstAsync();
        Assert.Equal(result, token.Get<int>());
    }

    [Test]
    [Arguments("1.5", 1.5)]
    [Arguments("-1.5", -1.5)]
    [Arguments("-.005", -0.005)]
    [Arguments("210.", 210)]
    [Arguments("1.25e2", 125)]
    [Arguments("1.25E+2", 125)]
    [Arguments("12500.0e-2", 125)]
    [Arguments("12500e-2", 125)]
    [Arguments("12500E-2", 125)]
    public async Task ParseFloatAsync(string source, float result)
    {
        using var sut = CreateTokenizer(source);
        var token = await sut.TokensAsync().FirstAsync();
        Assert.Equal(result, token.Get<double>(), 4);
    }

    [Test]
    public async Task EnumerateTokensAsync()
    {
        using var sut = CreateTokenizer("123.4 true false (Hello World)");
        Assert.Equal(4, await sut.TokensAsync().CountAsync());
    }

    private static Tokenizer CreateTokenizer(string code)
    {
        return new Tokenizer(new MemoryReader(Encoding.ASCII.GetBytes(code)));
    }

    private static Tokenizer CreateSyncTokenizer(string code)
    {
        return new Tokenizer(code);
    }
}