using System;
using System.Buffers;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// This decodes hexadecimal strings
/// </summary>
public readonly struct HexStringDecoder : IStringDecoder<byte>
{
    /// <inheritdoc />
    public int DecodeFrom(
        ref SequenceReader<byte> source, scoped Span<byte> destination, ref byte state)
    {
        if (!source.TryReadNextVisible(out var highChar)) return -1;
        if (highChar is (byte)'>') return 0;
        
        if (!source.TryReadNextVisible(out var lowChar)) return -1;
        if (lowChar is (byte)'>')
        {
            destination[0] = CharacterClassifier.ByteFromHexChars(highChar, (byte)'0');
            source.Rewind(1);
            return 1;
        }
        destination[0] = CharacterClassifier.ByteFromHexChars(highChar, lowChar);
        return 1;
    }

    /// <inheritdoc />
    public int MaxCharsPerBlock => 1;

}