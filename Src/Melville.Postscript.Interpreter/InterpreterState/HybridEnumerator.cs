using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.InterpreterState;

/// <summary>
/// This is effectively a union type that allows code to rely on either synchronous or
/// asynchronous enumeration, with the carevat that an underlying async enumerator cannot
/// be consumed in synchronous code, but synchronous enumeration can be consumed in synchronous
/// or asynchronous code.
/// </summary>
/// <typeparam name="T">The type of the enumerated values.</typeparam>
public readonly partial struct HybridEnumerator<T>
{
    /// <summary>
    /// The object being enumerated.
    /// </summary>
    public object InnerEnumerator { get; }

    /// <summary>
    /// Create a HybridEnumerator from an AsyncEnumerator
    /// </summary>
    /// <param name="enumerator">The source enumerator</param>
    public HybridEnumerator(IAsyncEnumerator<T> enumerator) => InnerEnumerator = enumerator;
    
    /// <summary>
    /// Create a HybridEnumerator from an AsyncEnumerator
    /// </summary>
    /// <param name="enumerator">The source enumerator</param>
    public HybridEnumerator(IEnumerator<T> enumerator) => InnerEnumerator = enumerator;

    /// <summary>
    /// The current element of the enumeration
    /// </summary>
    public T Current => InnerEnumerator switch
    {
        IAsyncEnumerator<T> ae => ae.Current,
        IEnumerator<T> e => e.Current,
        _ => throw new ArgumentOutOfRangeException()
    };

    /// <summary>
    /// Attempt to retrieve the next enumerated value, if one exists
    /// </summary>
    /// <returns>True if the new value exiss, false otherwise</returns>
    public ValueTask<bool> MoveNextAsync() => InnerEnumerator switch
    {
        IAsyncEnumerator<T> ae => ae.MoveNextAsync(),
        IEnumerator<T> e => new(e.MoveNext()),
        _ => throw new ArgumentOutOfRangeException()
    };

    /// <summary>
    /// Attempt to retrieve the next enumerated value, if one exists
    /// </summary>
    /// <returns>True if the new value exiss, false otherwise</returns>
    public bool MoveNext() => InnerEnumerator switch
    {
        IAsyncEnumerator<T> ae => throw new InvalidOperationException("Cannot synchronously read async enumerator"),
        IEnumerator<T> e => e.MoveNext(),
        _ => throw new ArgumentOutOfRangeException()
    };

    /// <summary>
    /// If this value is true, then calling MoveNext will return false.
    /// If this function returns false, then calling MoveNext might return
    /// true or false.
    /// </summary>
    /// <returns>True if the next movenext is known to be false,
    /// false if the next move will be true or is unknown.</returns>
    public bool NextMoveNextWillBeFalse() => 
        InnerEnumerator is IPropheticEnumerator pe && pe.NextMoveNextWillBeFalse();
}