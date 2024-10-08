﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melville.Parsing.ObjectRentals;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;
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
    [InlineData("exch pop", "exch", "pop")]
    public async Task Parse2NamesAsync(string source, string name1, string name2)
    {
        using var sut = CreateTokenizer(source);
        var list = await sut.TokensAsync().ToListAsync();
        Assert.Equal(2, list.Count);
        VerifyToken(list[0], name1);
        VerifyToken(list[1], name2);
    }

    [Fact]
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
        using var tokenizer = CreateTokenizer(source);
        var token = await tokenizer.TokensAsync().FirstAsync();
        Assert.Equal(result, token.ToString());
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
            PostscriptValueFactory.CreateString(decoded, StringKind.String).ToString());

    [Fact]
    public Task ExceptionForMismatchedCloseWakkaAsync() =>
        Assert.ThrowsAsync<PostscriptNamedErrorException>(()=>
        {
            using var tokenizer = CreateTokenizer(">");
            return tokenizer.TokensAsync().FirstAsync().AsTask();
        });

    [Theory]
    [InlineData("1234", 1234)]
    [InlineData("+1234", 1234)]
    [InlineData("-1234", -1234)]
    [InlineData("16#A", 10)]
    [InlineData("16#Ab9", 0xab9)]
    [InlineData("2#1001001", 0b1001001)]
    public async Task ParseIntsAsync(string source, int result)
    {
        using var sut = CreateTokenizer(source);
        var token = await sut.TokensAsync().FirstAsync();
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
        using var sut = CreateTokenizer(source);
        var token = await sut.TokensAsync().FirstAsync();
        Assert.Equal(result, token.Get<double>(), 4);
    }

    [Fact]
    public async Task EnumerateTokensAsync()
    {
        using var sut = CreateTokenizer("123.4 true false (Hello World)");
        Assert.Equal(4, await sut.TokensAsync().CountAsync());
    }

    private static Tokenizer CreateTokenizer(string code)
    {
        return new Tokenizer(
            new MemoryStream(
                Encoding.ASCII.GetBytes(code)));
    }

    private static Tokenizer CreateSyncTokenizer(string code)
    {
        return new Tokenizer(code);
    }
}