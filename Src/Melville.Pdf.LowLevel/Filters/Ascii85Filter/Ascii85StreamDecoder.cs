using System;
using System.Buffers;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Filters.Ascii85Filter;

internal class Ascii85StreamDecoder : IStreamFilterDefinition
{
    public int MinWriteSize => 4;

    private ReadOnlySpan<byte> TerminatorSequence() => "~>"u8;

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done)
        Convert(ref SequenceReader<byte> source, in Span<byte> destination)
    {
        var state = new byte();
        var decoder = new Ascii85Decoder();
        var partialDestination = destination;
        var consumedPosition = source.Position;
        int totalBytesWritten = 0;
        while (partialDestination.Length >= decoder.MaxCharsPerBlock)
        {
            var localBytesWritten = decoder.DecodeFrom(ref source, partialDestination, ref state);
            if (localBytesWritten == 0) return (consumedPosition, totalBytesWritten, true);
            if (localBytesWritten < 0) return (consumedPosition, totalBytesWritten, false);
            consumedPosition = source.Position;
            totalBytesWritten += localBytesWritten;
            partialDestination = partialDestination[localBytesWritten..];
        }

        return (consumedPosition, totalBytesWritten, false);

    }

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(
        ref SequenceReader<byte> source,
        in Span<byte> destination)
    {
        var seqLen = (int)source.Length + TerminatorSequence().Length;
        var bufferArray = ArrayPool<byte>.Shared.Rent(seqLen);
        var buffer = bufferArray.AsMemory(0, seqLen);
        try
        {
            var bufferSpan = buffer.Span;
            var terminatorLength = TerminatorSequence().Length;
            source.TryCopyTo(bufferSpan[..^terminatorLength]);
            TerminatorSequence().TryCopyTo(bufferSpan[^terminatorLength..]);
            var buffSeq = new ReadOnlySequence<byte>(buffer);
            var buffReader = new SequenceReader<byte>(buffSeq);
            var (finalPos, bytesWritten, done) = Convert(ref buffReader, destination);
            return (
                source.Sequence.GetPosition(Math.Min(source.Sequence.Length,
                    buffSeq.Slice(buffSeq.Start, finalPos).Length)),
                bytesWritten, done);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bufferArray);
        }
    }
}