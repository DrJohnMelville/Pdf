using System;
using Melville.Postscript.Interpreter.InterpreterState;

namespace Melville.Postscript.Interpreter.Values.Execution;

internal abstract class BuiltInFunction : IExternalFunction
{
    public abstract void Execute(PostscriptEngine engine, in PostscriptValue value);

    string IPostscriptValueStrategy<string>.GetValue(in Int128 memento) =>
        "<Built in Function>";

    public virtual string WrapTextDisplay(string text) => text;

    public virtual bool IsExecutable => true;
}