using System;
using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.Values;

[StaticSingleton]
internal partial class StringExecutor : IExecutePostscript
{
    public void Execute(PostscriptEngine engine, in PostscriptValue value)
    {
        var tokenizer = new Tokenizer(value.Get<Memory<byte>>());
        engine.ExecutionStack.Push(new(tokenizer.Tokens().GetEnumerator()), value);

    }

    public string WrapTextDisplay(string text) => text;
    public bool IsExecutable => true;
}
