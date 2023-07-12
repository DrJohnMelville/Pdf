using System;
using Melville.Postscript.Interpreter.Values.Strings;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// Try to encode a string into its postscript interpreter implementation
/// </summary>
public static class StringEncoder
{
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
}