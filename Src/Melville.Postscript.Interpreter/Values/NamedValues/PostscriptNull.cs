using System;
using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.Values;

[StaticSingleton]
internal partial class PostscriptNull : 
    IPostscriptValueStrategy<string>, IPostscriptValueStrategy<IExecutionSelector>
{
    public string GetValue(in Int128 memento) => "<Null>";

    IExecutionSelector IPostscriptValueStrategy<IExecutionSelector>.GetValue(in Int128 memento) =>
        NullExecutionSelector.Instance;
}

[StaticSingleton()]
internal sealed partial class NullExecutionSelector : IExecutionSelector, IExecutePostscript
{
    public IExecutePostscript Literal => PostscriptBuiltInOperations.PushArgument;
    public IExecutePostscript Executable => this;

    public void Execute(PostscriptEngine engine, in PostscriptValue value)
    {
        // do nothing
    }

    public string WrapTextDisplay(string text) => text;

    public bool IsExecutable => true;
}