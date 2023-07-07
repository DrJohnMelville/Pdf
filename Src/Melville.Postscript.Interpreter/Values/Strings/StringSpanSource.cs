using System;
using System.Runtime.InteropServices;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// Copy a postscript string into a span.
/// </summary>
public unsafe partial struct StringSpanSource
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
    /// This buffer holds the expanded characters from a shortstring
    /// </summary>
    private fixed byte buffer[PostscriptString.ShortStringLimit];

    /// <summary>
    /// Returns the given string as a span.  This might be the internal private buffer, or
    /// it might be the native span of a long string.
    /// </summary>
    public Span<byte> GetSpan() =>
        strategy.GetBytes(memento, MemoryMarshal.CreateSpan(ref buffer[0], PostscriptString.ShortStringLimit));
}