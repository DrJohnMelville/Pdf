using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// This interface lets us know sometimes that there is no more data before
/// we call MoveNext -- this one space worth of prophecy allows us to do tail
/// call elimination on procedures.
/// </summary>
/// <typeparam name="T"></typeparam>
internal interface IPropheticEnumerator
{
    /// <summary>
    ///This interface is meant to be implemented on something that also implements
    /// IEnumerator&lt;T&gt;
    /// 
    /// If this value is true, then calling MoveNext will return false.
    /// If this function returns false, then calling MoveNext might return
    /// true or false.
    /// </summary>
    /// <returns>True if the next MoveNext is known to be false,
    /// false if the next move will be true or is unknown.</returns>
    public bool NextMoveNextWillBeFalse();

}

internal partial class MemoryEnumerator : IEnumerator<PostscriptValue>, IPropheticEnumerator
{
    [FromConstructor] private readonly Memory<PostscriptValue> values;
    private int nextPosition = 0;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public bool MoveNext()
    {
        if (NextMoveNextWillBeFalse()) return false;
        Current = values.Span[nextPosition++];
        return true;
    }

    public bool NextMoveNextWillBeFalse() => nextPosition >= values.Length;

    public void Reset() => nextPosition = 0;

    object IEnumerator.Current => Current;

    public PostscriptValue Current { get; private set; }
    public void Dispose()
    {
    }

}