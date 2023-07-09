using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;

namespace Melville.Postscript.Interpreter.Values.Execution;

/// <summary>
/// This class represents C# code that can be called from postscript
/// </summary>
public abstract class BuiltInFunction : IExternalFunction
{
    /// <inheritdoc />
    public abstract void Execute(PostscriptEngine engine, in PostscriptValue value);

    string IPostscriptValueStrategy<string>.GetValue(in MementoUnion memento) =>
        "<Built in Function>";

    /// <inheritdoc />
    public virtual string WrapTextDisplay(string text) => text;

    /// <inheritdoc />
    public virtual bool IsExecutable => true;
}

/// <summary>
/// This class represents asynchronous c# code that can be called from postscript
/// </summary>
public abstract class AsyncBuiltInFunction : BuiltInFunction, IExecutePostscript
{
    /// <inheritdoc />
    public override void Execute(PostscriptEngine engine, in PostscriptValue value) =>
        throw new NotSupportedException(
            "This task cannot be executed by a synchronous interpreter");

    /// <inheritdoc />
    public abstract ValueTask ExecuteAsync(PostscriptEngine engine, in PostscriptValue value);
}

