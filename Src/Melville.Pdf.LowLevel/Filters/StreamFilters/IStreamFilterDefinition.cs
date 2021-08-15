using System;
using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.StreamFilters
{
    public interface IStreamFilterDefinition
    {
        (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
            ref SequenceReader<byte> source, ref Span<byte> destination);

        (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(
            ref SequenceReader<byte> source, ref Span<byte> destination) =>
            (source.Position, 0, true);
        int MinWriteSize => 1;
    }
}