using System;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.Postscript.Interpreter.Values.Strings;

namespace Melville.Postscript.Interpreter.Values;

[FromConstructor]
internal sealed partial class PostscriptShortString : PostscriptString
{
    protected override Span<byte> GetBytes(in Int128 memento, in Span<byte> scratch)
    {
        Int128 remainingChars = memento;
        int i;
        for (i = 0; i < scratch.Length && remainingChars != 0; i++)
        {
            scratch[i] = SevenBitStringEncoding.GetNextCharacter(ref remainingChars);
        }
        return scratch[..i];
    }

    //The PostscriptValue object uses the strategy hash as part of its own hash,
    // which it combines with the memento to get the hashcode of the PostscriptValue.
    // Because the StringKind should not affect eqality, we need to make all the shortstring
    // providers have the same hash so only the memento will differ.
    public override int GetHashCode() => 12345;
}