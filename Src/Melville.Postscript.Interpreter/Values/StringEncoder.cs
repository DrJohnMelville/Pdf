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
        if (data.Length > PostscriptString.ShortStringLimit)
            return AsLongString(data, kind);
        ;
        UInt128 value = 0;
        for (int i = data.Length - 1; i >= 0; i--)
        {
            var character = data[i];
            if (character is 0 or > 127) return AsLongString(data, kind);
            SevenBitStringEncoding.AddOneCharacter(ref value, character);
        }

        return (kind.ShortStringStraegy, new MementoUnion(value));

    }

    private static (object Strategy, MementoUnion memento) AsLongString(
        in ReadOnlySpan<byte> data, StringKind kind) =>
        new(new PostscriptLongString(kind, data.ToArray()), default);

    /// <summary>
    /// Create a Postscript string / memento pair from two ulongs.  This is
    /// used by the generator to create the precomputed values
    /// </summary>
    /// <param name="kind">The stringKind</param>
    /// <param name="low">The low order bytes</param>
    /// <param name="high">The high order bytes</param>
    /// <returns>The strategy and memento for the encoded string</returns>
    public static (object Strategy, MementoUnion Memento) FromULongs(
        StringKind kind, ulong low, ulong high) =>
        (kind.ShortStringStraegy, MementoUnion.CreateFrom(low, high));
}