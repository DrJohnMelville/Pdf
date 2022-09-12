using System;
using System.Buffers;
using Melville.Parsing.StreamFilters;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

public class EveryThirdByteFilter : IStreamFilterDefinition
{
    public static IStreamFilterDefinition Instance = new EveryThirdByteFilter();
    private EveryThirdByteFilter() {}

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(ref SequenceReader<byte> source, in Span<byte> destination)
    {
        var destPos = 0;
        while (destPos < destination.Length && source.TryPeek(2, out var currentByte))
        {
            source.Advance(3);
            destination[destPos++] = currentByte;
        }

        return (source.Position, destPos, false);
    }
}