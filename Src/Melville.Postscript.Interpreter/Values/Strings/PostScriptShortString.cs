using System;
using System.Buffers;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values.Strings;

namespace Melville.Postscript.Interpreter.Values;

[FromConstructor]
internal sealed partial class PostscriptShortString : PostscriptString
{
    internal override Span<byte> GetBytes(
        scoped in MementoUnion memento, scoped in Span<byte> scratch)
    {
        UInt128 remainingChars = memento.UInt128;
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

    private protected override PostscriptLongString AsLongString(in MementoUnion memento) => 
        new(StringKind, ValueAsMemory(memento));

    private protected override RentedMemorySource InnerRentedMemorySource(MementoUnion memento)
    {
        var array = ArrayPool<byte>.Shared.Rent(PostscriptString.ShortStringLimit);
        var span = GetBytes(memento, array.AsSpan());
        return new(array.AsMemory(0, span.Length), array);
    }

    private protected override Memory<byte> ValueAsMemory(in MementoUnion memento)
    {
        var value = GetBytes(memento, stackalloc byte[PostscriptString.ShortStringLimit]);
        return value.ToArray();
    }
}