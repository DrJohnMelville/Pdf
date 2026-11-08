using Melville.Parsing;
using Melville.Postscript.Interpreter.Values;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;

namespace Melville.Postscript.Interpreter.Tokenizers;


internal class SyntaxStringTokenizer: SpanTokenizer
{
    private static readonly SearchValues<byte> parens = SearchValues.Create(@"()"u8);
    protected override bool TryFindLength(ReadOnlySpan<byte> source, out int length)
    {
        length = 1;
        int balance = 1;
        while (balance > 0)
        {
            int nextPos = source[length..].IndexOfAny(parens);
            if (nextPos < 0) return (-1).AsFalseValue(out length);
            balance += TryPrefix(source, length + nextPos) switch
            {
                [(byte)'\\', (byte)'\\', (byte)'('] => 1,
                [(byte)'\\', (byte)'\\', (byte)')'] => -1,
                [.., (byte)'\\', _] => 0,
                [.. , (byte)'('] => 1,
                [.. , (byte)')'] => -1,
                _ => throw new InvalidOperationException("This should be impossible")
            };
            length += nextPos + 1;
        }
        return true;
    }

    public ReadOnlySpan<byte> TryPrefix(ReadOnlySpan<byte> input, int end) =>
        input[Math.Max(0, end - 2)..(end + 1)];
    protected override PostscriptValue DecodeSpan(ReadOnlySpan<byte> source)
    {
        var input = new SpanParsingBuffer(source[1..^1]);
        using var output = new SpanWriter(input.CurrentLength);

        while (input.CurrentLength > 0)
        {
            if (ReadCharacter(ref input) is { } charRead)
                output.Push(charRead);
        }
        return PostscriptValueFactory.CreateString(output.Value, StringKind.String);
    }
    byte? ReadCharacter(ref SpanParsingBuffer input)
    {
        return input.GetUInt8() switch
        {
            (byte)'\\' => ReadSlashCharacter(ref input),
            (byte)'\r' when input.Peek() is [(byte)'\n',..] => null,
            (byte)'\r' =>(byte)'\n',
            var other => other
        };
    }
    byte? ReadSlashCharacter(ref SpanParsingBuffer input)
    {
        if (!input.TryGetUInt8(out var second)) return null;
        return second switch
        {
            (byte)'n' => (byte)'\n',
            (byte)'r' => (byte)'\r',
            (byte)'t' => (byte)'\t',
            (byte)'b' => (byte)'\b',
            (byte)'f' => (byte)'\f',
            (byte)'\r' or (byte)'\n' => EatNewLine(ref input),
            >= (byte)'0' and <= (byte)'7' and var first => OutputOctal(ref input, first),
            var item => item
        };
    }
    byte? EatNewLine(ref SpanParsingBuffer input)
    {
        while (input.Peek() is [(byte)'\r', ..] or [(byte)'\n',..]) input.Advance(1);
        return null;
    }
    byte OutputOctal(ref SpanParsingBuffer input, byte first)
    {
        var firstValue = CharacterClassifier.ValueFromDigit(first);
        if (firstValue > 7) return 0;
        if (TryGetOctalDigit(ref input, out var middleValue))
        {
            if (TryGetOctalDigit(ref input, out var lowValue))
            {
                return ConstructOctal(firstValue, middleValue, lowValue);
            }
            return ConstructOctal(0, firstValue, middleValue);
        }
        return ConstructOctal(0,0,firstValue);
    }
    public bool TryGetOctalDigit(ref SpanParsingBuffer buffer, out byte value)
    {
      if (buffer.Peek() is [var low, ..] &&
                CharacterClassifier.ValueFromDigit(low) is var lowValue and < 8)
        {
            value = lowValue;
            buffer.Advance(1);
            return true;
        }
        value = 255;
        return false;
    }

    private byte ConstructOctal(byte high, byte middle, byte low) =>
        (byte)((64 * high) + (8 * middle) + low);
}