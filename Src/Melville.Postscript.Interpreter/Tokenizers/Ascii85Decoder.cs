using System;
using System.Buffers;

namespace Melville.Postscript.Interpreter.Tokenizers
{
    internal readonly struct Ascii85Decoder : IStringDecoder<byte>
    {
        public int DecodeFrom(
            ref SequenceReader<byte> source, scoped Span<byte> destination, ref byte state)
        {
            if (!source.TryReadNextVisible(out var byte1)) return -1;
            return byte1 switch
            {
                Ascii85Constants.FirstTerminatingChar => 0,
                Ascii85Constants.FourZeroChar => FillZeroBytes(destination),
                _ => DecodeQuintuple(ref source, destination, byte1)
            };
        }

        private int FillZeroBytes(Span<byte> destination)
        {
            destination[..4].Clear();
            return 4;
        }

        private int DecodeQuintuple(
            ref SequenceReader<byte> source, scoped Span<byte> destination, byte firstChar)
        {
            var quint = new Ascii85Quintuple();
            quint.AddByte(firstChar);
            for (int i = 0; i < 4; i++)
            {
                if (!source.TryReadNextVisible(out var nextChar)) return -1;
                if (nextChar is Ascii85Constants.FirstTerminatingChar)
                {
                    source.Rewind(1);
                    return quint.WriteDecodedBytesPartial(destination, i+1);
                }
                quint.AddByte(nextChar);
            }

            return quint.WriteDecodedBytes(destination, 4);
        }

        public int MaxCharsPerBlock => 4;
    }
}