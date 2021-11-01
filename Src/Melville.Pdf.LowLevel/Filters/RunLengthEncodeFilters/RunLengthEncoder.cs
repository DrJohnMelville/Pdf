using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.StreamFilters;

namespace Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters;

public class RunLengthEncoder : IStreamFilterDefinition
{
    public int MinWriteSize => 129;
            
    public (SequencePosition SourceConsumed, int bytesWritten, bool Done)
        Convert(ref SequenceReader<byte> source, ref Span<byte> destination) =>
        new RleEncoderEngine(source, destination).Convert(false);

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(
        ref SequenceReader<byte> source, ref Span<byte> destination) =>
        new RleEncoderEngine(source, destination).Convert(true);
}