using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.StreamFilters;

namespace Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters;

public class RunLengthDecoder : IStreamFilterDefinition
{
    public int MinWriteSize => 128;

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
        ref SequenceReader<byte> source, ref Span<byte> destination)
    {
        return new RleDecoderEngine(source, destination).Convert();
    }
}