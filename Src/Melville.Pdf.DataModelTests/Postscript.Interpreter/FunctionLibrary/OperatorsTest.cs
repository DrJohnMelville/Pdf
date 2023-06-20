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
    
    [Theory]
    [InlineData("1 2 3 2 packedarray", "01: [2 3]\r\n02: 1")]
    [InlineData("true setpacking currentpacking", "01: true")]
    [InlineData("currentpacking", "01: false")]
    [InlineData("3 array", "01: [<Null> <Null> <Null>]")]
    [InlineData("23[1    2 \r\n 3]", "01: [1 2 3]\r\n02: 23")]
    [InlineData("[1 2 3] length", "01: 3")]
    [InlineData("[1 2 3] 1 get ", "01: 2")]
    [InlineData("[1 2 3] dup 2 /true put", "01: [1 2 /true]")]
    [InlineData("[1 2 3] 1 2 getinterval", "01: [2 3]")]
    [InlineData("[1 2 3] dup 1 [8 9] putinterval", "01: [1 8 9]")]
    [InlineData("1 2 3 4 count array astore", "01: [1 2 3 4]")]
    [InlineData("1 [2 3 4 5] aload ", """
        01: [2 3 4 5]
        02: 5
        03: 4
        04: 3
        05: 2
        06: 1
        """)]
    [InlineData("[1 2 3] 5 array dup 3 1 roll copy", """
        01: [1 2 3]
        02: [1 2 3 <Null> <Null>]
        """)]
    public Task WithArrayOperatorsAsync(string code, string result) =>
        RunTestOnAsync(code, result, new PostscriptEngine()
            .WithStackOperators().WithArrayOperators().WithSystemTokens());

    [Theory]
    [InlineData("/add cvx", "01: add")]
    [InlineData("/add cvx cvlit", "01: /add")]
    [InlineData("5 2 /mul cvx exec", "01: 10")]
    [InlineData("5 xcheck", "01: false")]
    [InlineData("(string) xcheck", "01: false")]
    [InlineData("1.2 xcheck", "01: false")]
    [InlineData("true xcheck", "01: false")]
    [InlineData("/Name xcheck", "01: false")]
    [InlineData("/Name cvx xcheck", "01: true")]
    [InlineData("{add} xcheck", "01: true")]
    [InlineData("1 executeonly", "01: 1")]
    [InlineData("1 readonly", "01: 1")]
    [InlineData("1 noaccess", "01: 1")]
    [InlineData("1 rcheck", "01: true")]
    [InlineData("1 wcheck", "01: true")]
    [InlineData("123.4 cvi", "01: 123")]
    [InlineData("123 cvr", "01: 123")]
    [InlineData("(123.4) cvi", "01: 123")]
    [InlineData("(Hello) cvn", "01: /Hello")]
    [InlineData("(Hello) cvx cvn", "01: Hello")]
    [InlineData("/Hello cvs", "01: (/Hello)")]
    [InlineData("(TESTING) dup 123 10 3 -1 roll cvrs", "01: (123)\r\n02: (123TING)")]
    [InlineData("(TESTING) dup -123 10 3 -1 roll cvrs", "01: (-123)\r\n02: (-123ING)")]
    [InlineData("(TESTING) dup 123.4 10 3 -1 roll cvrs", "01: (123.4)\r\n02: (123.4NG)")]
    [InlineData("(TESTING) dup 123 16 3 -1 roll cvrs", "01: (7B)\r\n02: (7BSTING)")]
    [InlineData("(TESTINGX) dup -123 16 3 -1 roll cvrs", "01: (FFFFFF85)\r\n02: (FFFFFF85)")]
    [InlineData("(TESTING) dup 123.4 16 3 -1 roll cvrs", "01: (7B)\r\n02: (7BSTING)")]
    [InlineData("(TESTING) dup 0 16 3 -1 roll cvrs", "01: (0)\r\n02: (0ESTING)")]
    [InlineData("0 [1 2 3] {add} forall", "01: 6")]
    public Task ExecutionAndConversionAsync(string code, string result) =>
        RunTestOnAsync(code, result, new PostscriptEngine()
            .WithcConversionOperators().WithcControlOperators().WithMathOperators()
            .WithSystemTokens().WithArrayOperators().WithStackOperators());

    [Theory]
    [InlineData("10 exec", "01: 10")]
    [InlineData("/start cvx start", "01: start")]
    [InlineData("{2 3 add} exec", "01: 5")]
    [InlineData("2 { 3 add} exec", "01: 5")]
    [InlineData("(2 3 add) cvx exec", "01: 5")]
    [InlineData("true {1} if", "01: 1")]
    [InlineData("false {1} if", "")]
    [InlineData("true {1} {2} ifelse", "01: 1")]
    [InlineData("false {1} {2} ifelse", "01: 2")]
    [InlineData("0 1 1 4 {add} for", "01: 10")]
    [InlineData("1 2 6 {} for", "01: 5\r\n02: 3\r\n03: 1")]
    [InlineData("3 -.5 1 { } for", "01: 1\r\n02: 1.5\r\n03: 2\r\n04: 2.5\r\n05: 3")]
    [InlineData("4 {1} repeat", "01: 1\r\n02: 1\r\n03: 1\r\n04: 1")]
    [InlineData("4 {1 exit} repeat", "01: 1")]
    [InlineData("4 true false false false { {exit} if} loop", "01: 4")]
    [InlineData("1 {4 stop 5} stopped 6", "01: 6\r\n02: true\r\n03: 4\r\n04: 1")]
    [InlineData("1 {4 5} stopped 6", "01: 6\r\n02: false\r\n03: 5\r\n04: 4\r\n05: 1")]
    [InlineData("{countexecstack} exec", "01: 2")]
    [InlineData("{countexecstack array execstack stop} stopped",
        "01: true\r\n02: [Executed Code Stop Context [countexecstack array execstack stop]]")]

    [InlineData("1 { 2 { 3 quit} exec 4 } exec 5", "01: 3\r\n02: 2\r\n03: 1")]
    public Task ControlOperatorsAsync(string code, string result) =>
        RunTestOnAsync(code, result, new PostscriptEngine()
            .WithcConversionOperators().WithcControlOperators().WithMathOperators()
            .WithSystemTokens().WithArrayOperators().WithStackOperators());

    [Theory]
    [InlineData("1 1 ne", "01: false")]
    [InlineData("1 2 ne", "01: true")]
    [InlineData("1 1 eq", "01: true")]
    [InlineData("1.0 1 eq", "01: true")]
    [InlineData("1.1 1 eq", "01: false")]
    [InlineData("1 2 eq", "01: false")]
    [InlineData("1 2 eq", "01: false")]
    [InlineData("/H1 /H1 eq", "01: true")]
    [InlineData("/H1 /H2 eq", "01: false")]
    [InlineData("(H1) dup pop /H1 eq", "01: true")]
    [InlineData("/H1 /H1 cvx eq", "01: true")]
    [InlineData("[1 2 3] dup eq", "01: true")]
    [InlineData("[1 2 3] [1 2 3] eq", "01: false")]
    [InlineData("true not", "01: false")]
    [InlineData("false not", "01: true")]
    [InlineData("0 not", "01: -1")]
    [InlineData("2 not", "01: -3")]

    [InlineData("2 1 ge", "01: true")]
    [InlineData("1 1 ge", "01: true")]
    [InlineData("0 1 ge", "01: false")]
    [InlineData("2 1 gt", "01: true")]
    [InlineData("1 1 gt", "01: false")]
    [InlineData("0 1 gt", "01: false")]
    [InlineData("2 1 le", "01: false")]
    [InlineData("1 1 le", "01: true")]
    [InlineData("0 1 le", "01: true")]
    [InlineData("2 1 lt", "01: false")]
    [InlineData("1 1 lt", "01: false")]
    [InlineData("0 1 lt", "01: true")]
    [InlineData("1.1 1 gt", "01: true")]
    [InlineData("/B /A gt", "01: true")]

    [InlineData("3 5 and", "01: 1")]
    [InlineData("false false and", "01: false")]
    [InlineData("false true and", "01: false")]
    [InlineData("true false and", "01: false")]
    [InlineData("true true and", "01: true")]
    [InlineData("3 5 or", "01: 7")]
    [InlineData("false false or", "01: false")]
    [InlineData("false true or", "01: true")]
    [InlineData("true false or", "01: true")]
    [InlineData("true true or", "01: true")]
    [InlineData("3 5 xor", "01: 6")]
    [InlineData("false false xor", "01: false")]
    [InlineData("false true xor", "01: true")]
    [InlineData("true false xor", "01: true")]
    [InlineData("true true xor", "01: false")]

    [InlineData("4 2 bitshift", "01: 16")]
    [InlineData("4 -2 bitshift", "01: 1")]
    public Task RelationalAndBitwise(string code, string result) =>
        RunTestOnAsync(code, result, new PostscriptEngine()
            .WithcRelationalOperators()
            .WithSystemTokens()
            .WithStackOperators()
            .WithArrayOperators()
            .WithcConversionOperators());

    private static async Task RunTestOnAsync(string code, string result, PostscriptEngine engine)
    {
        await engine.ExecuteAsync(new Tokenizer(code));
        Assert.Equal(result, engine.OperandStack.ToString());
    }
}