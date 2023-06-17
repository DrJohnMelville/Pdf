using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

internal partial class AsyncMemoryEnumerator : IAsyncEnumerator<PostscriptValue>
{
    [FromConstructor] private readonly Memory<PostscriptValue> values;
    private int nextPosition = 0;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public ValueTask<bool> MoveNextAsync()
    {
        if (nextPosition >= values.Length) return ValueTask.FromResult(false);
        Current = values.Span[nextPosition++];
        return ValueTask.FromResult(true);
    }

    public PostscriptValue Current { get; private set; }
}