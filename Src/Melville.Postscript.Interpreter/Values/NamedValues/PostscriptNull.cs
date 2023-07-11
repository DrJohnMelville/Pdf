using System;
using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.Values;

// A PostscriptValueStrategy representing a null object
[StaticSingleton]
public partial class PostscriptNull : 
    IPostscriptValueStrategy<string>, IExecutionSelector, IExecutePostscript
{
    /// <inheritdoc />
    public string GetValue(in MementoUnion memento) => "null";

    /// <inheritdoc />
    public IExecutePostscript Literal => PostscriptBuiltInOperations.PushArgument;

    /// <inheritdoc />
    public IExecutePostscript Executable => this;

    /// <inheritdoc />
    public void Execute(PostscriptEngine engine, in PostscriptValue value)
    {
        // do nothing
    }

    /// <inheritdoc />
    public string WrapTextDisplay(string text) => text;

    /// <inheritdoc />
    public bool IsExecutable => true;
}