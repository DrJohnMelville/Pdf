using System;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// Try to encode a string into its postscript interpreter implementation
/// </summary>
public static class StringEncoder
{
    /// <summary>
    /// Encode a string as a strategy/memento pair
    /// </summary>
    /// <param name="data">The string data returned.</param>
    /// <param name="kind">The StringKind of the returned string type/</param>
    /// <returns>The strategy and memento that encodes the given time.</returns>
    public static (object Strategy, MementoUnion memento) PostscriptEncode(
        this in ReadOnlySpan<byte> data, StringKind kind)
    {
        if (kind.Strategy8Bit.TryEncode(data, out var memento))
            return (kind.Strategy8Bit, memento);
        if (kind.Strategy6Bit.TryEncode(data, out memento))
            return (kind.Strategy6Bit, memento);
        if (kind.Strategy6Bit.TryEncode(data, out memento))
            return (kind.Strategy6Bit, memento);
        return AsLongString(data, kind);
    }

    private static (object Strategy, MementoUnion memento) AsLongString(
        in ReadOnlySpan<byte> data, StringKind kind) =>
        new(new PostscriptLongString(kind, data.ToArray()), default);
}