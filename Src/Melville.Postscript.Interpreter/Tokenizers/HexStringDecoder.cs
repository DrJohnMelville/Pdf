using System;
using System.Buffers;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.Tokenizers
{
    internal readonly struct HexStringDecoder : IStringDecoder<byte>
    {
        public int DecodeFrom(
            ref SequenceReader<byte> source, scoped Span<byte> destination, ref byte state)
        {
            if (!source.TryReadNextVisible(out var highChar)) return -1;
            if (highChar is (byte)'>') return 0;
        
            if (!source.TryReadNextVisible(out var lowChar)) return -1;
            if (lowChar is (byte)'>')
            {
                destination[0] = ByteFromChars(highChar,(byte)'0');
                source.Rewind(1);
                return 1;
            }
            destination[0] = ByteFromChars(highChar, lowChar);
            return 1;
        }

        private static byte ByteFromChars(byte highChar, byte lowChar) => 
            (byte)((ConvertByte(highChar) << 4) | ConvertByte(lowChar));

        private static byte ConvertByte(byte hiByte)
        {
            hiByte = CharacterClassifier.ValueFromDigit(hiByte);
            if (hiByte > 15) throw new PostscriptParseException("Invalid char in hex string");
            return hiByte;
        }

        public int MaxCharsPerBlock => 1;

    }
}