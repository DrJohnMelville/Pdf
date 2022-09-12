using System;
using System.Buffers;
using Melville.Parsing.StreamFilters;

namespace Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters;

public class RunLengthDecoder : IStreamFilterDefinition
{
    public int MinWriteSize => 128;

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
        ref SequenceReader<byte> source, in Span<byte> destination)
    {
        return new RleDecoderEngine(source, destination).Convert();
    }
}