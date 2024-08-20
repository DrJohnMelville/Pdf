using System;
using System.Collections;
using System.Collections.Generic;
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
        engine.ExecutionStack.Push(new(CreateTokenEnumerator(value)), value);
    }

    private static DisposingEnumerator<PostscriptValue> CreateTokenEnumerator(PostscriptValue value)
    {
        var tokenizer = new Tokenizer(value.Get<Memory<byte>>());
        var disposingEnumerator = new DisposingEnumerator<PostscriptValue>(tokenizer.Tokens().GetEnumerator(),
            tokenizer);
        return disposingEnumerator;
    }

    public string WrapTextDisplay(string text) => text;
    public bool IsExecutable => true;
}

internal partial class DisposingEnumerator<T> : IEnumerator<T>
{
    [FromConstructor] [DelegateTo] private readonly IEnumerator<T> inner;
    [FromConstructor] private IDisposable? disposeWhenDone;

    public bool MoveNext()
    {
        if (inner.MoveNext()) return true;
        DisposeOfTarget();
        return false;
    }

    private void DisposeOfTarget()
    {
        disposeWhenDone?.Dispose();
        disposeWhenDone = null;
    }

    object IEnumerator.Current => inner.Current!;

    public T Current => inner.Current;
}
