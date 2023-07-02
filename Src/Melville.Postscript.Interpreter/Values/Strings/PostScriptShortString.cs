using System;
using System.Buffers;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.Postscript.Interpreter.Values.Strings;

namespace Melville.Postscript.Interpreter.Values;

[FromConstructor]
internal sealed partial class PostscriptShortString : PostscriptString
{
    internal override Span<byte> GetBytes(
        scoped in Int128 memento, scoped in Span<byte> scratch)
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

    private protected override PostscriptLongString AsLongString(in Int128 memento) => 
        new(StringKind, ValueAsMemory(memento));

    private protected override RentedMemorySource InnerRentedMemorySource(Int128 memento)
    {
        var array = ArrayPool<byte>.Shared.Rent(PostscriptString.ShortStringLimit);
        var span = GetBytes(memento, array.AsSpan());
        return new(array.AsMemory(0, span.Length), array);
    }

    private protected override Memory<byte> ValueAsMemory(in Int128 memento)
    {
        var value = GetBytes(memento, stackalloc byte[PostscriptString.ShortStringLimit]);
        return value.ToArray();
    }
}