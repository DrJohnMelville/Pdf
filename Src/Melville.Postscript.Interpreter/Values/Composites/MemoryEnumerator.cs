using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

internal partial class MemoryEnumerator : IEnumerator<PostscriptValue>
{
    [FromConstructor] private readonly Memory<PostscriptValue> values;
    private int nextPosition = 0;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public bool MoveNext()
    {
        if (nextPosition >= values.Length) return false;
        Current = values.Span[nextPosition++];
        return true;
    }

    public void Reset()
    {
        nextPosition = 0;
    }

    object IEnumerator.Current => Current;

    public PostscriptValue Current { get; private set; }
    public void Dispose()
    {
    }
}