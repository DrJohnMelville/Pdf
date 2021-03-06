using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Filters.Ascii85Filter;

public static class Ascii85Constants
{
    public const byte FirstChar = (byte)'!';
    public const byte IncompleteGroupPadding = (byte)'u';
    public const byte FirstTerminatingChar = (byte)'~';
    public const byte SecondTerminatingChar = (byte)'>';
}

public class Ascii85Decoder : IStreamFilterDefinition
{
    public int MinWriteSize => 4;
        
    public (SequencePosition SourceConsumed, int bytesWritten, bool Done)
        Convert(ref SequenceReader<byte> source, in Span<byte> destination)
    {
        var destPosition = 0;
        while (true)
        {
            var lastPosition = source.Position;
            if (destination.Length - destPosition < 4)
                return (lastPosition, destPosition, false);
            if (!source.TryReadNonWhitespace(out byte b1))
                return (lastPosition, destPosition, false);
            if (b1 == Ascii85Constants.FirstTerminatingChar) return (lastPosition, destPosition, true);
            if (b1 == (byte)'z')
            {
                destPosition = WriteQuad(destination, 0, 4, destPosition);
            }
            else
            {
                if (!(source.TryReadNonWhitespace(out var b2) && source.TryReadNonWhitespace(out var b3) &&
                      source.TryReadNonWhitespace(out var b4) && source.TryReadNonWhitespace(out var b5)))
                    return (lastPosition, destPosition, false);
                bool isDone;
                (destPosition, isDone) = HandleQuintuple(b1, b2, b3, b4, b5, destination, destPosition);
                if (isDone) return (source.Position, destPosition, true);
            }
        }
    }

    private (int, bool) HandleQuintuple(byte b1, byte b2, byte b3, byte b4, byte b5,
        in Span<byte> destination, int destPosition) => (b1, b2, b3, b4, b5) switch
    {
        (_, Ascii85Constants.FirstTerminatingChar, _, _, _) =>
            (WriteQuad(destination, ComputeQuad(b1), 0, destPosition), true),
        (_, _, Ascii85Constants.FirstTerminatingChar, _, _) =>
            (WriteQuad(destination, ComputeQuad(b1, b2), 1, destPosition), true),
        (_, _, _, Ascii85Constants.FirstTerminatingChar, _) =>
            (WriteQuad(destination, ComputeQuad(b1, b2, b3), 2, destPosition), true),
        (_, _, _, _, Ascii85Constants.FirstTerminatingChar) =>
            (WriteQuad(destination, ComputeQuad(b1, b2, b3, b4), 3, destPosition), true),
        _ => (WriteQuad(destination, ComputeQuad(b1, b2, b3, b4, b5), 4, destPosition), false)
    };

    private uint ComputeQuad(
        byte b1 = Ascii85Constants.IncompleteGroupPadding,
        byte b2 = Ascii85Constants.IncompleteGroupPadding,
        byte b3 = Ascii85Constants.IncompleteGroupPadding,
        byte b4 = Ascii85Constants.IncompleteGroupPadding,
        byte b5 = Ascii85Constants.IncompleteGroupPadding)
    {
        uint ret = (uint)b1 - Ascii85Constants.FirstChar;
        ret *= 85;
        ret += (uint)b2 - Ascii85Constants.FirstChar;
        ret *= 85;
        ret += (uint)b3 - Ascii85Constants.FirstChar;
        ret *= 85;
        ret += (uint)b4 - Ascii85Constants.FirstChar;
        ret *= 85;
        ret += (uint)b5 - Ascii85Constants.FirstChar;
        return ret;

    }

    private int WriteQuad(Span<byte> destination, uint data, int toWrite, int destPosition)
    {
        for (int i = toWrite; i < 4; i++)
        {
            data >>= 8;
        }

        for (int i = toWrite - 1; i >= 0; i--)
        {
            destination[destPosition + i] = (byte)(data & 0xFF);
            data >>= 8;
        }

        return destPosition + toWrite;
    }

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(
        ref SequenceReader<byte> source,
        in Span<byte> destination)
    {
        if (!source.TryReadNonWhitespace(out var b1) || b1 == Ascii85Constants.FirstTerminatingChar)
            return (source.Sequence.Start, 0, false);
        if (!source.TryReadNonWhitespace(out var b2) || b2 == Ascii85Constants.FirstTerminatingChar)
        {
            throw new PdfParseException("Single character group in a Ascii85 stream");
        }

        if (!source.TryReadNonWhitespace(out var b3))
        {
            b3 = Ascii85Constants.FirstTerminatingChar;
        }

        if (!source.TryReadNonWhitespace(out var b4))
        {
            b4 = Ascii85Constants.FirstTerminatingChar;
        }

        var (pos, done) =
            HandleQuintuple(b1, b2, b3, b4, Ascii85Constants.FirstTerminatingChar, destination, 0);
        return (source.Position, pos, done);
    }
}