using Melville.Postscript.Interpreter.InterpreterState;
using System.Threading.Tasks;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.Tokenizers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Postscript.Interpreter.FunctionLibrary;

public class OperatorsTest
{
    [Theory]
    [InlineData("true", "01: true")]
    [InlineData("false", "01: false")]
    [InlineData("null", "01: <Null>")]
    public Task TestSystemTokensAsync(string code, string result) => 
        RunTestOnAsync(code, result, new PostscriptEngine().WithSystemTokens());

    [Theory]
    [InlineData("4 5 pop", "01: 4")]
    [InlineData("4 5 exch", "01: 4\r\n02: 5")]
    [InlineData("4 dup", "01: 4\r\n02: 4")]
    [InlineData("1 2 3 2 copy", """
        01: 3
        02: 2
        03: 3
        04: 2
        05: 1
        """)]
    [InlineData("1 4 3 1 index", """
        01: 4
        02: 3
        03: 4
        04: 1
        """)]
    [InlineData("1 2 3 4 5   3 1 roll", """
        01: 4
        02: 3
        03: 5
        04: 2
        05: 1
        """)]
    [InlineData("1 2 3 4 5   3 2 roll", """
        01: 3
        02: 5
        03: 4
        04: 2
        05: 1
        """)]
    [InlineData("1 2 3 4 5   3 -1 roll", """
        01: 3
        02: 5
        03: 4
        04: 2
        05: 1
        """)]
    [InlineData("1 2 3 4 5 3 -1 clear", "")]
    [InlineData("1 2 3 4 5 count", """
        01: 5
        02: 5
        03: 4
        04: 3
        05: 2
        06: 1
        """)]
    [InlineData("1 mark", """
        01: <Mark Object>
        02: 1
        """)]
    [InlineData("1 mark 2 3 4 counttomark", """
        01: 3
        02: 4
        03: 3
        04: 2
        05: <Mark Object>
        06: 1
        """)]
    [InlineData("1 mark 2 3 4 cleartomark", "01: 1")]
    public Task WithStackOperatorsAsync(string code, string result) =>
        RunTestOnAsync(code, result, new PostscriptEngine().WithStackOperators());

    [Theory]
    [InlineData("1 2 add", "01: 3")]
    [InlineData("1.5 2 add", "01: 3.5")]
    [InlineData("1 2.5 add", "01: 3.5")]
    [InlineData("1.25 2.25 add", "01: 3.5")]
    [InlineData("8 4 div", "01: 2")]
    [InlineData("9 4 div", "01: 2.25")]
    [InlineData("8 4 idiv", "01: 2")]
    [InlineData("9 4 idiv", "01: 2")]
    [InlineData("9 4 mul", "01: 36")]
    [InlineData("9 4 mod", "01: 1")]
    [InlineData("9 4 sub", "01: 5")]
    [InlineData("9 abs", "01: 9")]
    [InlineData("9.5 abs", "01: 9.5")]
    [InlineData("-9 abs", "01: 9")]
    [InlineData("-9.5 abs", "01: 9.5")]
    [InlineData("7 neg", "01: -7")]
    [InlineData("-7 neg", "01: 7")]
    [InlineData("9.1 ceiling", "01: 10")]
    [InlineData("-9.1 ceiling", "01: -9")]
    [InlineData("9.1 floor", "01: 9")]
    [InlineData("-9.1 floor", "01: -10")]
    [InlineData("9.4 round", "01: 9")]
    [InlineData("9.5 round", "01: 10")]
    [InlineData("9.5 truncate", "01: 9")]
    [InlineData("-9.5 truncate", "01: -9")]
    [InlineData("9 sqrt", "01: 3")]
    [InlineData("10 10 atan", "01: 45")]
    [InlineData("0 sin", "01: 0")]
    [InlineData("90 sin", "01: 1")]
    [InlineData("90 cos", "01: 6.123233995736766E-17")]
    [InlineData("0 cos", "01: 1")]
    [InlineData("10 2 exp", "01: 100")]
    [InlineData("100 ln", "01: 4.605170185988092")]
    [InlineData("234 srand rand rand rrand", """
        01: 1927566503
        02: 1927566503
        03: 11295414
        """)]
    public Task WithMathOperatorsAsync(string code, string result) =>
        RunTestOnAsync(code, result, new PostscriptEngine().WithMathOperators());

    private static async Task RunTestOnAsync(string code, string result, PostscriptEngine engine)
    {
        await engine.ExecuteAsync(new Tokenizer(code));
        Assert.Equal(result, engine.OperandStack.ToString());
    }
}