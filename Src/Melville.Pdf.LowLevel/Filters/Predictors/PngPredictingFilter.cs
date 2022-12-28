using System;
using System.Buffers;
using Melville.Parsing.StreamFilters;

namespace Melville.Pdf.LowLevel.Filters.Predictors;

internal abstract class PngPredictingFilter: IStreamFilterDefinition
{
    protected PngPredictionBuffer Buffer;

    protected PngPredictingFilter(int colors, int bitsPerColor, int columns)
    {
        Buffer = new PngPredictionBuffer(colors, bitsPerColor, columns);
    }

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
        ref SequenceReader<byte> source, in Span<byte> destination)
    {
        int written;
        for (written = 0;
             written < destination.Length && TryGetByte(ref source, out var nextByte);
             written++)
        {
            destination[written] = nextByte;
        }

        return (source.Position, written, false);
    }

    protected abstract bool TryGetByte(ref SequenceReader<byte> source, out byte byteToWrite);
}