using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Filters.AshiiHexFilters
{
    public class AsciiHexDecoder : IDecoder
    {
        public Stream WrapStream(Stream input, PdfObject parameter) => 
            new AsciiHexAdapter(input.AsPipeReader());

        private class AsciiHexAdapter: DecodingAdapter
        {
            public AsciiHexAdapter(PipeReader source) : base(source)
            {
            }

            public override (SequencePosition SourceConsumed, int bytesWritten, bool Done) 
                Decode(ref SequenceReader<byte> source, ref Span<byte> destination)
            {
                int position = 0;
                SequencePosition consumed = source.Sequence.Start;
                while (position < destination.Length&&
                       source.TryReadNonWhitespace(out var highByte) && 
                       source.TryReadNonWhitespace(out var lowByte))
                {
                    if (highByte == (byte) '>') return (consumed, position, true);
                    destination[position++] = HexMath.ByteFromHexCharPair(highByte, lowByte);
                    consumed = source.Position;
                    if (lowByte == (byte) '>') return (consumed, position, true);
                }
                return (consumed, position, false);
            }

            public override (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalDecode(ref SequenceReader<byte> source,
                ref Span<byte> destination)
            {
                if (destination.Length > 0 && source.TryReadNonWhitespace(out var highByte)
                  && HexMath.ByteToNibble(highByte) != Nibble.Terminator)
                {
                    destination[0] = HexMath.ByteFromHexCharPair(highByte, (byte) '0');
                    return (source.Position, 1, true);
                }

                return (source.Sequence.Start, 0, false);
            }
        }
    }
}