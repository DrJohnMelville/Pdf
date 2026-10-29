using Melville.StreamInterfaces.ParsingBuffers;
using System.Buffers;
using System.Reflection;

namespace Melville.Parsing.StreamFilters;

/// <summary>
/// An interface that represents a filter that compresses or decompresses a stream.
/// </summary>
public interface IStreamFilterDefinition
{
#warning get rid of the obsolete methods
    // oid -- obsolete methods

    /// <summary>
    /// Convert a segment of a source stream to the output stream.
    /// </summary>
    /// <param name="source">A sequence reader with source data.</param>
    /// <param name="destination">A span of bytes to receive the compressed or decompressed bytes.</param>
    /// <returns>A tuple of the next byte to be consumed, number of bytes written to the destination, and a bool that can be set to true if
    /// the filter has generated all of the available output.</returns>
    (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(
        ref SequenceReader<byte> source, in Span<byte> destination)
    {
#warning very inefficient
        var span = source.UnreadSequence.ToArray();
        var endDest = destination;
        var reader = new SpanParsingBuffer(span);
        var done = this.Convert(ref reader, ref endDest);
        source.Advance(reader.RelativePosition);
        return (source.UnreadSequence.Start, destination.Length - endDest.Length, done);
    }


    /// <summary>
    /// Convert a segment of a source stream to the output stream.  This method implies there is no more input to process.
    /// </summary>
    /// <param name="source">A sequence reader with source data.</param>
    /// <param name="destination">A span of bytes to receive the compressed or decompressed bytes.</param>
    /// <returns>A tuple of the next byte to be consumed, number of bytes written to the destination, and a bool that can be set to true if
    /// the filter has generated all of the available output.</returns>
    (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(
        ref SequenceReader<byte> source, in Span<byte> destination)
    {
#warning very inefficient
        var span = source.UnreadSequence.ToArray();
        var endDest = destination;
        var reader = new SpanParsingBuffer(span);
        var done = this.Convert(ref reader, ref endDest);
        source.Advance(reader.RelativePosition);
        return (source.UnreadSequence.Start, destination.Length - endDest.Length, done);
    }

    // new methods

    /// <summary>
    /// Convert source bytes to an output buffer.
    /// 
    /// Both parameters are ref parameters.  Advancing the input buffer indicates that those
    /// bytes have been fully consumed.  The output variable should be left at the
    /// first bute of the buffer passed in that is not filled.
    /// </summary>
    /// <param name="input">The sourece data.</param>
    /// <param name="output">The span to which the output should be routed.</param>
    /// <returns>True if the filter has no more data to provide, false otherwise.</returns>
    bool Convert(ref SpanParsingBuffer input, ref Span<byte> output)
    {
#warning very inefficient
        var oldLength = input.CurrentLength;
        var innerInput = new SequenceReader<byte>(new ReadOnlySequence<byte>(input.Peek().ToArray()));

            var (endPos, written, done) = this.Convert(ref innerInput, output);

        input.Advance((int)innerInput.Consumed);
        output = output[written..];
        return done;

    }

    /// <summary>
    /// Convert source bytes to an output buffer.
    /// 
    /// Both parameters are ref parameters.  Advancing the input buffer indicates that those
    /// bytes have been fully consumed.  The output variable should be left at the
    /// first bute of the buffer passed in that is not filled.
    /// </summary>
    /// <param name="input">The sourece data.</param>
    /// <param name="output">The span to which the output should be routed.</param>
    /// <returns>True if the filter has no more data to provide, false otherwise.</returns>
    bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output)
    {
#warning very inefficient
        var oldLength = input.CurrentLength;
        var innerInput = new SequenceReader<byte>(new ReadOnlySequence<byte>(input.Peek().ToArray()));

        var (endPos, written, done) = this.FinalConvert(ref innerInput, output);

        input.Advance((int)innerInput.Consumed);
        output = output[written..];
        return done;

    }

    /// <summary>
    /// Minimum size of a buffer that can be written to.
    /// </summary>
    int MinWriteSize => 1;
}