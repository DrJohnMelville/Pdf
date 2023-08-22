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
        return StringKind.IsMutable ? 
            new PostscriptValue(AsLongString(memento), StringKind.DefaultAction, default) : 
            new PostscriptValue(this, StringKind.DefaultAction, memento);
    }

    public abstract bool TryEncode(in ReadOnlySpan<byte> input, out MementoUnion memento);
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
            MementoUnion.CreateNameWithLength(input).AsTrueValue(out memento) : 
            default(MementoUnion).AsFalseValue(out memento);
}
[FromConstructor]
internal sealed partial class Postscript16ByteString8Bit : PostscriptShortString
{
    internal override Span<byte> GetBytes(scoped in MementoUnion memento, scoped in Span<byte> scratch)
    {
        var bytes = memento.Bytes;
        bytes.CopyTo(scratch);
        return scratch[..16];
    }
    public override bool TryEncode(in ReadOnlySpan<byte> input, out MementoUnion memento)
    {
        return input.Length == 16
            ? MementoUnion.CreateFromBytes(input).AsTrueValue(out memento)
            : default(MementoUnion).AsFalseValue(out memento);
    }
}

[FromConstructor]
sealed partial class PostscriptShortString6Bit: PostscriptShortString
{
    public const int MaxLength = 21;

    internal override Span<byte> GetBytes(scoped in MementoUnion memento, scoped in Span<byte> scratch)
    {
        var accumulator = memento.UInt128;
        int i = 0;
        for (; accumulator > 0 ; i++, accumulator >>=6)
        {
            scratch[i] = SixToEightBits((int)accumulator & 0b111111);
        }

        return scratch[..i];
    }
    public override bool TryEncode(in ReadOnlySpan<byte> input, out MementoUnion memento)
    {
        memento = default;
        if (input.Length is < 1 or > MaxLength || input[^1] == 0)
            return false;
        UInt128 accumulator = 0;
        for (int i = input.Length-1; i >= 0; i--)
        {
            if (!TryEightToSixBits(input[i], out var bits)) return false;
            accumulator = (accumulator << 6) | bits;

        }
        memento = new MementoUnion(accumulator);
        return true;
    }

    private byte SixToEightBits(int value) => value switch
    {
        1 => (byte)'-',
        >= 2 and <=11 => (byte)('0'-2 + value),
        >= 12 and <=37 => (byte)('A'-12 + value),
        >= 38 and <= 63 => (byte)('a'-38 + value),
        _ => 0    
    };

    private bool TryEightToSixBits(byte eightBits, out byte sixBits) => eightBits switch
    {
        0 => ((byte)0).AsTrueValue(out sixBits),
        (byte)'-' => ((byte)1).AsTrueValue(out sixBits), 
        >= (byte) '0' and <= (byte)'9' => ((byte)(2+eightBits-'0')).AsTrueValue(out sixBits),
        >= (byte) 'A' and <= (byte)'Z' => ((byte)(12+eightBits-'A')).AsTrueValue(out sixBits),
        >= (byte) 'a' and <= (byte)'z' => ((byte)(38+eightBits-'a')).AsTrueValue(out sixBits),
        _=> ((byte)0).AsFalseValue(out sixBits)
    };
}