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
        using var output = new SpanWriter(input.RemainingLength);

        while (input.RemainingLength > 0)
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

/// <summary>
/// Parses Postscript (and PDF) syntax strings.  This class uses an integer state parameter
/// to track the current parenthesis nesting level.
/// </summary>
[Obsolete("Use SyntaxStringTokenizer -- its faster.")]
public readonly struct SyntaxStringDecoder : IStringDecoder<int>
{
    /// <inheritdoc />
    public int DecodeFrom(
        ref SequenceReader<byte> source, scoped Span<byte> destination, ref int state)
    {
        if (!source.TryRead(out var character)) return -1;
        return character switch
        {
            (byte)'\\' => ProcessSlash(ref source, destination, ref state),
            (byte)'(' => OpenParen(destination, ref state),
            (byte)')' => CloseParen(destination, ref state),
            (byte)'\r' => RemoveImpliedCarriageReturn(ref source, destination),
            _ => OutputSingle(destination, character)
        };
    }

    private int RemoveImpliedCarriageReturn(
        ref SequenceReader<byte> source, scoped Span<byte> destination)
    {
        if (!source.TryPeek(out var character)) return -1;
        if (character is (byte) '\n') source.Advance(1);
        return OutputSingle(destination, (byte)'\n');
    }

    private int ProcessSlash(
        ref SequenceReader<byte> source, scoped Span<byte> destination, ref int state)
    {
        if (!source.TryRead(out var character)) return -1;
        return character switch
        {
            (byte)'n' => OutputSingle(destination, (byte)'\n'),
            (byte)'r' => OutputSingle(destination, (byte)'\r'),
            (byte)'t' => OutputSingle(destination, (byte)'\t'),
            (byte)'b' => OutputSingle(destination, (byte)'\b'),
            (byte)'f' => OutputSingle(destination, (byte)'\f'),
            (byte)'\r' or (byte)'\n' => EatNewLine(ref source, destination, state),
            >= (byte)'0' and <= (byte)'7' => OutputOctal(ref source, destination, character),
            _ => OutputSingle(destination, character)
        };
    }

    private int OutputOctal(
        ref SequenceReader<byte> source, scoped Span<byte> destination, byte firstCharacter)
    {
        var firstDigit = CharacterClassifier.ValueFromDigit(firstCharacter);

        if (!source.TryRead(out var secondChar)) return -1;
        var secondDigit = CharacterClassifier.ValueFromDigit(secondChar);
        if (secondDigit > 7)
        {
            source.Rewind(1);
            return OutputSingle(destination, firstDigit);
        }

        if (!source.TryRead(out var thirdChar)) return -1;
        var thirdDigit = CharacterClassifier.ValueFromDigit(thirdChar);
        if (thirdDigit > 7)
        {
            source.Rewind(1);
            return OutputSingle(destination, FromOctal(0, firstDigit, secondDigit));
        }
        return OutputSingle(destination, FromOctal(firstDigit, secondDigit, thirdDigit));
    }

    private byte FromOctal(byte a, byte b, byte c) => (byte)((64*a)+(8*b)+c);


    private int EatNewLine(
        ref SequenceReader<byte> source, scoped Span<byte> destination, int state)
    {
        byte character;
        do
        {
            if (!source.TryRead(out character)) return -1;
        } while (CharacterClassifier.IsLineEnd(character));

        source.Rewind(1);
        return DecodeFrom(ref source, destination, ref state);
    }

    private int OpenParen(in Span<byte> destination, ref int state)
    {
        state++;
        return OutputSingle(destination, (byte)'(');
    }

    private int CloseParen(in Span<byte> destination, ref int state)
    {
        if (state < 1) return 0;
        state--;
        return OutputSingle(destination, (byte)')');
    }

    private int OutputSingle(in Span<byte> destination, byte character)
    {
        destination[0] = character;
        return 1;
    }

    /// <inheritdoc />
    public int MaxCharsPerBlock => 1;
}