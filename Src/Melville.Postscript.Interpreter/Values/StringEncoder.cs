using System;
using Melville.Postscript.Interpreter.Values.Strings;

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
        if (kind.Strategy7Bit.TryEncode(data, out memento))
            return (kind.Strategy7Bit, memento);
        return AsLongString(data, kind);
    }

    private static (object Strategy, MementoUnion memento) 
        Encode8BitString(in ReadOnlySpan<byte> data, StringKind kind) =>
        (kind.Strategy8Bit, MementoUnion.CreateFrom(data));

    private static (object Strategy, MementoUnion memento) Encode7BitString(
        ReadOnlySpan<byte> input, StringKind kind)
    {
        UInt128 value = 0;
        for (int i = input.Length - 1; i >= 0; i--)
        {
            var character = input[i];
            if (character is 0 or > 127) return AsLongString(input, kind);
            SevenBitStringEncoding.AddOneCharacter(ref value, character);
        }

        return (kind.Strategy7Bit, new MementoUnion(value));
    }

    private static (object Strategy, MementoUnion memento) AsLongString(
        in ReadOnlySpan<byte> data, StringKind kind) =>
        new(new PostscriptLongString(kind, data.ToArray()), default);
}