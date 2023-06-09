using System;
using System.Buffers;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Parses Postscript (and PDF) syntax strings.  This class uses an integer state parameter
/// to track the current parenthesis nesting level.
/// </summary>
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
        } while (CharacterClassifier.IsLineEndChar(character));

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