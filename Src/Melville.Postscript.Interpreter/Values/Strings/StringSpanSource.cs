using System;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// Copy a postscript string into a span.
/// </summary>
public readonly partial struct StringSpanSource
{
    /// <summary>
    /// The stringstrategy for this string
    /// </summary>
    [FromConstructor] private readonly PostscriptString strategy;
    /// <summary>
    /// the memento which might hold the string data.
    /// </summary>
    [FromConstructor] private readonly Int128 memento;

    /// <summary>
    /// Returns the given string as a span.  This might be the supplied span, or
    /// it might be the native span of a long string.
    /// </summary>
    /// <param name="scratch">A readonly span that can optionally be used as a buffer</param>
    public Span<byte> GetSpan(Span<byte> scratch) =>
        strategy.GetBytes(memento, scratch);
}