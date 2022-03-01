using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Filters.AsciiHexFilters;

public class AsciiHexDecoder : IStreamFilterDefinition
{
    public (SequencePosition SourceConsumed, int bytesWritten, bool Done)
        Convert(ref SequenceReader<byte> source, in Span<byte> destination)
    {
        int position = 0;
        SequencePosition consumed = source.Sequence.Start;
        while (position < destination.Length &&
               source.TryReadNonWhitespace(out var highByte) &&
               source.TryReadNonWhitespace(out var lowByte))
        {
            if (highByte == (byte)'>') return (consumed, position, true);
            destination[position++] = HexMath.ByteFromHexCharPair(highByte, lowByte);
            consumed = source.Position;
            if (lowByte == (byte)'>') return (consumed, position, true);
        }

        return (consumed, position, false);
    }

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(
        ref SequenceReader<byte> source,
        in Span<byte> destination)
    {
        if (destination.Length > 0 && source.TryReadNonWhitespace(out var highByte)
                                   && HexMath.ByteToNibble(highByte) != Nibble.Terminator)
        {
            destination[0] = HexMath.ByteFromHexCharPair(highByte, (byte)'0');
            return (source.Position, 1, true);
        }

        return (source.Sequence.Start, 0, false);
    }
}