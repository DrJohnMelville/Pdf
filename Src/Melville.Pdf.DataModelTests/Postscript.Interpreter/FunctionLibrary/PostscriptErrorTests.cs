using System.Threading.Tasks;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Xunit;

namespace Melville.Pdf.DataModelTests.Postscript.Interpreter.FunctionLibrary;

public class PostscriptErrorTests
{
    [Fact]
    public void UndefinedErrorTest()
    {
        var engine = new PostscriptEngine();
        engine.Execute("1 JDM 1");
        AssertErrorString(engine, "newerror"u8, "true");
        AssertErrorString(engine, "errorname"u8, "undefined");
        AssertErrorString(engine, "command"u8, "JDM");
        AssertErrorString(engine, "ostack"u8, "[1]");
        AssertErrorString(engine, "estack"u8, "[Synchronous CodeSource]");
        AssertErrorString(engine, "dstack"u8, """
               [<<
                   errordict: <<
               >>
                   $error: <<
                   newerror: true
                   errorname: undefined
                   command: JDM
                   ostack: [1]
                   estack: [Synchronous CodeSource]
                   dstack: <Blocked Recursive String write.>
               >>
                   statusdict: <<
               >>
               >> <<
               >> <<
               >>]
               """);
    }

    [Fact]
    public async Task UndefinedErrorTestAsync()
    {
        var engine = new PostscriptEngine();
        await engine.ExecuteAsync("1 JDM 1");
        AssertErrorString(engine, "newerror"u8, "true");
        AssertErrorString(engine, "errorname"u8, "undefined");
        AssertErrorString(engine, "command"u8, "JDM");
        AssertErrorString(engine, "ostack"u8, "[1]");
        AssertErrorString(engine, "estack"u8, "[Async Parser]");
        AssertErrorString(engine, "dstack"u8, """
               [<<
                   errordict: <<
               >>
                   $error: <<
                   newerror: true
                   errorname: undefined
                   command: JDM
                   ostack: [1]
                   estack: [Async Parser]
                   dstack: <Blocked Recursive String write.>
               >>
                   statusdict: <<
               >>
               >> <<
               >> <<
               >>]
               """);
    }

    private void AssertErrorString(
        PostscriptEngine engine, PostscriptValue itemName, string expectedValue)
    {
        var actual = engine.ErrorData.GetAs<string>(itemName);
        Assert.Equal(expectedValue, actual);
    }

    [Theory]
    [InlineData("1 JDM", "undefined", "[1]")]
    [InlineData("end", "dictstackunderflow", "[]")]
    [InlineData("{2 dict begin} loop", "dictstackoverflow", "[<<\r\n>>]")]
    [InlineData(" /proc { proc 1 } def proc", "execstackoverflow", "[]")]
    [InlineData("add", "stackunderflow", "[]")]
    [InlineData("1 add", "stackunderflow", "[1]")]
    [InlineData("/JDM 1 add", "typecheck", "[/JDM 1]")]
    [InlineData("1 1 >>", "unmatchedmark", "[1 1]")]
    public void ErrorDefinitions(string code, string errorName, string operandStack)
    {
        var engine = new PostscriptEngine().WithBaseLanguage();
        engine.Execute(code);
        AssertErrorString(engine, "newerror"u8, "true");
        AssertErrorString(engine, "errorname"u8, errorName);
        AssertErrorString(engine, "ostack"u8, operandStack);
    }

    [Fact]
    public void InvalidExitPreservesExecutionStack()
    {
        var engine = new PostscriptEngine().WithBaseLanguage();
        engine.Execute("/proc { exit 1} def proc 1");
        AssertErrorString(engine, "estack"u8, "[Synchronous CodeSource [exit 1]]");

    }

    [Fact] public void OperandStackOverflow()
    {
        var engine = new PostscriptEngine().WithBaseLanguage();
        engine.Execute("{1} loop");
        AssertErrorString(engine, "errorname"u8, "stackoverflow");
        AssertErrorString(engine, "estack"u8, "[Synchronous CodeSource Loop Loop]");

    }
}