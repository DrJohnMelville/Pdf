using System;
using System.Buffers;
using System.Diagnostics;
using Melville.INPC;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Postscript.Interpreter.Values.Strings;

[FromConstructor]
internal abstract partial class PostscriptShortString : PostscriptString, IMakeCopyableInstance
{
    private protected override PostscriptLongString AsLongString(in MementoUnion memento) => 
        new(StringKind, ValueAsMemory(memento));

    private protected override RentedMemorySource InnerRentedMemorySource(MementoUnion memento)
    {
        var array = ArrayPool<byte>.Shared.Rent(PostscriptString.ShortStringLimit);
        var span = GetBytes(memento, array.AsSpan());
        return new(array.AsMemory(0, span.Length), array);
    }

    private protected override Memory<byte> ValueAsMemory(in MementoUnion memento) => 
        new StringSpanSource(this, memento).GetSpan().ToArray();

    public PostscriptValue AsCopyableValue(in MementoUnion memento)
    {
        Span<byte> buffer = stackalloc byte[PostscriptString.ShortStringLimit];
        return PostscriptValueFactory.CreateLongString(
            GetBytes(memento, buffer).ToArray(), StringKind);
    }

    public abstract bool TryEncode(in ReadOnlySpan<byte> input, out MementoUnion memento);
}

[FromConstructor]
internal sealed partial class PostscriptShortString7Bit : PostscriptShortString
{
    public const int MaxLength = 18;
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

    public override bool TryEncode(in ReadOnlySpan<byte> input, out MementoUnion memento)
    {
        memento = default;
        if (input.Length > MaxLength ||
            (input.Length > 0 && input[^1] == 0))
            return false;
        
        UInt128 value = 0;
        for (int i = input.Length - 1; i >= 0; i--)
        {
            var character = input[i];
            if (character > 127) return false;
            SevenBitStringEncoding.AddOneCharacter(ref value, character);
        }

        memento = new MementoUnion(value);
        return true;
    }
}

[FromConstructor]
internal sealed partial class PostscriptShortString8Bit : PostscriptShortString
{
    public const int MaxLength = 15;
    internal override Span<byte> GetBytes(scoped in MementoUnion memento, scoped in Span<byte> scratch)
    {
        var bytes = memento.Bytes;
        int length = bytes[0];
        Debug.Assert(length is >= 0 and <= MaxLength);
        bytes.Slice(1, length).CopyTo(scratch);
        return scratch[..length];
    }
    public override bool TryEncode(in ReadOnlySpan<byte> input, out MementoUnion memento) =>
        input.Length <= MaxLength ? 
            MementoUnion.CreateFrom(input).AsTrueValue(out memento) : 
            default(MementoUnion).AsFalseValue(out memento);
}

[FromConstructor]
sealed partial class PostscriptShortString6Bit: PostscriptShortString
{
    public const int MaxLength = 21;

    internal override Span<byte> GetBytes(scoped in MementoUnion memento, scoped in Span<byte> scratch)
    {
        throw new NotImplementedException();
    }
    public override bool TryEncode(in ReadOnlySpan<byte> input, out MementoUnion memento)
    {
        throw new NotImplementedException();
    }

    private bool TrySixBits (byte eightBits, out byte sixBits) => eightBits switch
    {
        0 => ((byte)0).AsTrueValue(out sixBits),
        (byte)'-' => ((byte)1).AsTrueValue(out sixBits),
        >= (byte) '0' and <= (byte)'9'; Start here
    }
}