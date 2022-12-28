using System;
using System.Buffers;

namespace Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters;

internal ref struct RleDecoderEngine
{
    private SequenceReader<byte> source;
    private Span<byte> destination;
    private int destPosition;

    public RleDecoderEngine(SequenceReader<byte> source, Span<byte> destination) : this()
    {
        this.source = source;
        this.destination = destination;
        destPosition = 0;
    }

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert()
    {
        while (true)
        {
            if (!source.TryPeek(out var controlByte)) return (source.Position, destPosition, false);
            switch (controlByte)
            {
                case RleConstants.EndOfStream: return (source.Position, destPosition, true);
                case < RleConstants.EndOfStream:
                    int len = controlByte + 1;
                    if (SourceTooShort(len+1) || DestinationTooShort(len))
                        return (source.Position, destPosition, false);
                    WriteLiteralRun(len);
                    break;
                case > RleConstants.EndOfStream:
                    int repeats = RleConstants.RepeatedRunLength(controlByte);
                    if (DestinationTooShort(repeats) || !source.TryPeek(1, out var repeatedByte))
                        return (source.Position, destPosition, false);
                    WriteRepeatedRun(repeats, repeatedByte);
                    break;
            }                
        }
    }

    private void WriteRepeatedRun(int repeats, byte repeatedByte)
    {
        for (int i = 0; i < repeats; i++)
        {
            destination[destPosition++] = repeatedByte;
        }

        source.Advance(2);
    }

    private bool SourceTooShort(int len) => source.Remaining < len;

    private bool DestinationTooShort(int len) => len + destPosition >= destination.Length;

    private void WriteLiteralRun(int len)
    {
        source.Advance(1);
        source.TryCopyTo(destination.Slice(destPosition, len));
        source.Advance(len);
        destPosition += len;
    }
}