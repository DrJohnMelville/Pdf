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
    public void PushThreeInts()
    {
        var engine = ExcutedEngine("5 10 15");
        Assert.Equal(3, engine.OperandStack.Count);
        Assert.Equal(15, engine.OperandStack.Pop().Get<int>());
        Assert.Equal(10, engine.OperandStack.Pop().Get<int>());
        Assert.Equal(5, engine.OperandStack.Pop().Get<int>());
    }

    private static PostscriptEngine ExcutedEngine(string code)
    {
        var engine = new PostscriptEngine().WithSystemTokens();
        engine.Execute(code);
        return engine;
    }

    [Fact]
    public void PushThreeDoubles()
    {
        var engine = ExcutedEngine("15.1 10.2 15.");
        Assert.Equal(3, engine.OperandStack.Count);
        Assert.Equal(15, engine.OperandStack.Pop().Get<double>(), 1);
        Assert.Equal(10.2, engine.OperandStack.Pop().Get<double>(), 1);
        Assert.Equal(15.1, engine.OperandStack.Pop().Get<double>(), 1);
    }
    [Fact]
    public void PushOneString()
    {
        var engine = ExcutedEngine("(Hello)");
        Assert.Equal(1, engine.OperandStack.Count);
        Assert.Equal("(Hello)", engine.OperandStack.Pop().ToString());
    }

    [Fact]
    public void PushTwoLiteralNames()
    {
        var engine = ExcutedEngine("/Hello/World");
        Assert.Equal(2, engine.OperandStack.Count);
        Assert.Equal("/World", engine.OperandStack.Pop().ToString());
        Assert.Equal("/Hello", engine.OperandStack.Pop().ToString());
    }
    [Fact]
    public void ExecuteName()
    {
        var engine = ExcutedEngine("true");
        Assert.Equal(1, engine.OperandStack.Count);
        Assert.True(engine.OperandStack.Pop().Get<bool>());
    }
}