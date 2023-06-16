using System.Threading.Tasks;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;
using Xunit;

namespace Melville.Pdf.DataModelTests.Postscript.Interpreter.Execution;

public class ExecutionTest
{
    [Fact]
    public async Task PushThreeIntsAsync()
    {
        var tokens = new Tokenizer("5 10 15");
        var engine = new PostscriptEngine();
        await engine.ExecuteAsync(tokens);
        Assert.Equal(3, engine.OperandStack.Count);
        Assert.Equal(15, engine.OperandStack.Pop().Get<int>());
        Assert.Equal(10, engine.OperandStack.Pop().Get<int>());
        Assert.Equal(5, engine.OperandStack.Pop().Get<int>());
    }
    [Fact]
    public async Task PushThreeDoublesAsync()
    {
        var tokens = new Tokenizer("15.1 10.2 15.");
        var engine = new PostscriptEngine();
        await engine.ExecuteAsync(tokens);
        Assert.Equal(3, engine.OperandStack.Count);
        Assert.Equal(15, engine.OperandStack.Pop().Get<double>(), 1);
        Assert.Equal(10.2, engine.OperandStack.Pop().Get<double>(), 1);
        Assert.Equal(15.1, engine.OperandStack.Pop().Get<double>(), 1);
    }
    [Fact]
    public async Task PushOneStringAsync()
    {
        var tokens = new Tokenizer("(Hello)");
        var engine = new PostscriptEngine();
        await engine.ExecuteAsync(tokens);
        Assert.Single(engine.OperandStack);
        Assert.Equal("(Hello)", engine.OperandStack.Pop().ToString());
    }

    [Fact]
    public async Task PushTwoLiteralNamesAsync()
    {
        var tokens = new Tokenizer("/Hello/World");
        var engine = new PostscriptEngine();
        await engine.ExecuteAsync(tokens);
        Assert.Equal(2, engine.OperandStack.Count);
        Assert.Equal("/World", engine.OperandStack.Pop().ToString());
        Assert.Equal("/Hello", engine.OperandStack.Pop().ToString());
    }
    [Fact]
    public async Task ExecuteNameAsync()
    {
        var tokens = new Tokenizer("true");
        var engine = new PostscriptEngine().WithSystemTokens();
        await engine.ExecuteAsync(tokens);
        Assert.Single(engine.OperandStack);
        Assert.True(engine.OperandStack.Pop().Get<bool>());
    }
}