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

    private static async Task RunTestOnAsync(string code, string result, PostscriptEngine engine)
    {
        await engine.ExecuteAsync(new Tokenizer(code));
        Assert.Equal(result, engine.OperandStack.ToString());
    }
}